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
using OSIsoft.AF.Asset;
using OSIsoft.AF.Time;

namespace PIReplayLib
{
    /// <summary>
    ///     Encapsulates all interpolated AFValue(s) of source points of interest at a specific timestamp.
    /// </summary>
    public class DataRecord
    {
        public AFTime Time { get; set; }
        public IList<AFValue> Values { get; set; }

        public static DataRecord Create(AFTime time, IList<AFValue> values)
        {
            return new DataRecord {Time = time, Values = values};
        }
    }
}