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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;

namespace PIReplayLib
{
    public class PIReader
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (PIReader));
        private PIPointList _destPoints;

        private PIServer _destServer;
        private readonly int _interval = 10;

        private readonly int _lookAheadMinutes = 5;

        //private Timer _timer;
        private double _period = 120;

        private readonly DataQueue _queue;
        private PIReplayer _replayer;
        private readonly PIPointList _sourcePoints;

        private PIServer _sourceServer;

        public PIReader(PIReplayer replayer,
            PIServer sserver, PIPointList spoints,
            PIServer dserver, PIPointList dpoints,
            DataQueue queue)
        {
            _replayer = replayer;

            _sourceServer = sserver;
            _sourcePoints = spoints;

            _destServer = dserver;
            _destPoints = dpoints;


            _queue = queue;
        }

        public void GetPages(bool initial = false)
        {
            _logger.Info(string.Format("Entering {0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.Name,
                MethodBase.GetCurrentMethod().Name));
            GetPages(_queue.LatestTime.LocalTime, initial);
        }



        private void GetPages(DateTime startTime, bool initial = false)
        {
            var addMinutes = _lookAheadMinutes;
            if (initial) addMinutes = _lookAheadMinutes*2;

            var historicalStartTime = startTime.AddYears(-1);
            var historicalEndTime = historicalStartTime.AddMinutes(addMinutes);

            _logger.Info(string.Format("Getting page for {0} - {1}",
                historicalStartTime.AddYears(1), historicalEndTime.AddYears(1)));

            var timeRange = new AFTimeRange(new AFTime(historicalStartTime), new AFTime(historicalEndTime));
            var timeSpan = new AFTimeSpan(TimeSpan.FromSeconds(_interval), new AFTimeZone());

            // Transpose the returned IEnumerable<AFValues> (each list item has same PI Point) into 
            // IList<DataRecord> (each list item has same timestamp)
            IList<DataRecord> records = _sourcePoints
                .InterpolatedValues(timeRange, timeSpan, string.Empty, true,
                    new PIPagingConfiguration(PIPageType.TagCount, 1000))
                .Select(vals =>
                {
                    foreach (var v in vals)
                    {
                        v.Timestamp = v.Timestamp.LocalTime.AddYears(1);
                        ;
                    }
                    return vals;
                })
                .SelectMany(vals => vals)
                .GroupBy(v => v.Timestamp)
                .Select(grp => DataRecord.Create(grp.Key, grp.ToList()))
                .OrderBy(rec => rec.Time)
                .ToList();

            _queue.Add(records);
            _logger.Info(string.Format("Added {0} records", records.Count));
        }
    }
}