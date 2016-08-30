#region Copyright
//  Copyright 2016 Barry Shang / Patrice Thivierge F.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;
using Timer = System.Timers.Timer;

namespace PIReplayLib
{
    public class PIWriter
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (PIWriter));
        private readonly PIPointList _destPoints;

        private readonly PIServer _destServer;
        private readonly double _period = 5; // in seconds

        private readonly DataQueue _queue;

        private readonly PIReplayer _replayer;

        private Task _requestFill;
        private readonly PIPointList _sourcePoints;

        private PIServer _sourceServer;

        // Source and destination PI Points are matched simply by point name.
        private Dictionary<PIPoint, PIPoint> _sourceToDest;

        private readonly Timer _timer;

        public PIWriter(PIReplayer replayer,
            PIServer sserver, PIPointList spoints,
            PIServer dserver, PIPointList dpoints,
            DataQueue queue)
        {
            _replayer = replayer;

            _sourceServer = sserver;
            _sourcePoints = spoints;

            _destServer = dserver;
            _destPoints = dpoints;

            _timer = new Timer();
            _timer.Elapsed += WriteValues;
            _timer.Interval = Utils.FindInterval(_period);

            _queue = queue;
        }

        public void Start()
        {
            _timer.Start();

            // Request via the PIReplayer to fill the DataQueue. 
            // The PIReplayer will call the PIReader to read from source server and fill the queue.
            _requestFill = Task.Run(() => _replayer.RequestFill(true));

            _sourceToDest = new Dictionary<PIPoint, PIPoint>();

            var destPointNameLookup = _destPoints
                .GroupBy(p => p.Name)
                .ToDictionary(grp => grp.Key, grp => grp.First());

            foreach (var sourcePt in _sourcePoints)
            {
                _sourceToDest[sourcePt] = destPointNameLookup[sourcePt.Name];
            }
        }

        /// <summary>
        ///     This is called every 5 seconds via the timer callback.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WriteValues(object sender, ElapsedEventArgs e)
        {
            _logger.Info(string.Format("Current queue count: {0}", _queue.Count));
            _timer.Stop();

            // If data queue running low, request another fill.
            if (_queue.Count < 60)
            {
                if (_requestFill.IsCompleted || _requestFill.IsFaulted || _requestFill.IsCanceled)
                {
                    if (_requestFill.IsCanceled || _requestFill.IsFaulted)
                    {
                        _logger.Info("Cancelled or faulted");
                    }
                    _requestFill = Task.Run(() => _replayer.RequestFill());
                }
            }

            var syncTime = new AFTime(e.SignalTime);

            // Remove all records at and before the timer trigger (signal) time.
            var records = _queue.RemoveAtAndBefore(syncTime);

            _logger.Info(string.Format("Removed {0} records", records.Count));

            if (records.Count == 0)
            {
                _timer.Interval = Utils.FindInterval(_period);
                _logger.Info(string.Format("Next call in {0}", _timer.Interval));
                _timer.Start();
                return;
            }

            // Flatten the DataRecord in a list of AFValue(s)
            var valsList = records.SelectMany(rec => rec.Values).ToList();

            // Set the PIPoint property of the AFValue to the destination server PI Point
            foreach (var v in valsList)
            {
                v.PIPoint = _sourceToDest[v.PIPoint];
            }

            // Divide the AFValue list into 5 chunks. 
            // Wait 500 milliseconds before writing the next chunk to avoid sending too much data over the network at once.
            var chunkSize = valsList.Count/5;
            List<List<AFValue>> valsChunks = valsList.ChunkBy(chunkSize);

            var updated = 0;
            foreach (var chunk in valsChunks)
            {
                var errors = _destServer.UpdateValues(chunk, AFUpdateOption.InsertNoCompression, AFBufferOption.Buffer);

                if (errors != null && errors.HasErrors)
                {
                    foreach (var kvp in errors.Errors.Take(1))
                    {
                        _logger.Info(string.Format("Attr: {0}, Ex: {1}",
                            kvp.Key.Attribute.GetPath(), kvp.Value.Message));
                    }
                }
                else
                {
                    updated += chunk.Count;
                }
                Thread.Sleep(500);
            }
            _logger.Info(string.Format("Updated {0} tags", updated));

            _timer.Interval = Utils.FindInterval(_period);
            _logger.Info(string.Format("Next call in {0}", _timer.Interval));
            _timer.Start();
        }
    }

    public static class ListExtensions
    {
        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new {Index = i, Value = x})
                .GroupBy(x => x.Index/chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}