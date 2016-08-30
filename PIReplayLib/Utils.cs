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

namespace PIReplayLib
{
    public static class Utils
    {
        /// <summary>
        ///     Get the milliseconds to the next trigger time.
        ///     For example, if period = 5 seconds, we want the trigger times to be at 0, 5, 10, etc. seconds past the minute.
        ///     Ex. If the current time is at 12 seconds past the minute, then return 3000 milliseconds.
        ///     Therefore, the timer will trigger at 15 seconds past the minute.
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        public static double FindInterval(double period)
        {
            double totalMilliseconds;

            var currentTime = DateTime.Now;
            double seconds = currentTime.Second;
            var secondsToAdd = period - seconds%period;
            double milliseconds = currentTime.Millisecond;

            var projected = currentTime.AddSeconds(secondsToAdd).AddMilliseconds(-milliseconds);

            totalMilliseconds = (projected - currentTime).TotalMilliseconds;

            return totalMilliseconds;
        }

        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            return dateTime.AddTicks(-(dateTime.Ticks%timeSpan.Ticks));
        }

        public static IEnumerable<T> Do<T>(this IEnumerable<T> seq, Action<string> logger, string message)
        {
            logger(message);
            return seq;
        }
    }
}