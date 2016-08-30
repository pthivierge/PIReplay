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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using OSIsoft.AF.Time;

namespace PIReplayLib
{
    public class DataQueue
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (DataQueue));
        private readonly ConcurrentQueue<DataRecord> _queue;

        public DataQueue()
        {
            _queue = new ConcurrentQueue<DataRecord>();

            LatestTime = new AFTime(DateTime.Now.Truncate(TimeSpan.FromSeconds(1)));
        }

        public int Count
        {
            get { return _queue.Count; }
        }

        // Most recent timestamp of value in the queue.
        // This allows PIReader to know the start time of the next query.
        public AFTime LatestTime { get; private set; }

        public void Add(IList<DataRecord> records)
        {
            _logger.Info(string.Format("Entering {0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.Name,
                MethodBase.GetCurrentMethod().Name));
            foreach (var rec in records)
            {
                _queue.Enqueue(rec);
                if (LatestTime.CompareTo(rec.Time) < 0)
                {
                    LatestTime = rec.Time;
                }
            }
            _logger.Info(string.Format("Queue synced to {0}", LatestTime));
        }

        public IList<DataRecord> RemoveAtAndBefore(AFTime syncTime)
        {
            _logger.Info(string.Format("Entering {0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.Name,
                MethodBase.GetCurrentMethod().Name));
            var returnedRecs = new List<DataRecord>();

            var pointerRecTime = AFTime.MinValue;

            while (pointerRecTime.CompareTo(syncTime) <= 0 || _queue.Count == 0)
            {
                DataRecord rec = null;
                var peekResult = _queue.TryPeek(out rec);

                if (rec != null)
                {
                    pointerRecTime = rec.Time;
                    if (rec.Time.CompareTo(syncTime) <= 0)
                    {
                        rec = null;
                        _queue.TryDequeue(out rec);
                        if (rec != null)
                        {
                            returnedRecs.Add(rec);
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            return returnedRecs;
        }
    }
}