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
using log4net;
using PIReplayLib;
using CommandLine;
using OSIsoft.AF.Asset;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;

namespace PIReplayConsole
{
    internal class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));


        private static bool ValidateRunOptions(string[] options)
        { // [server] [TagSearchQuery]

            if (options.Length != 2)
                throw new Exceptions.InvalidParameterException("run", "The number of parameters with this option must be 2.");
            return true;
        }

        private static bool ValidateDeleteHistoryOptions(string[] options)
        { // [server] [delStartTime] [delEndTime] [TagSearchQuery]

            if (options.Length != 4)
                throw new Exceptions.InvalidParameterException("deleteHistory", "The number of parameters with this option must be 4.");

            // check the times passed, exception will be thrown is time passed is not valid
            AFTime.Parse(options[1]);
            AFTime.Parse(options[2]);

            return true;
        }

        private static void Main(string[] args)
        {
            try
            {



                var options = new CommandLineOptions();
                if (Parser.Default.ParseArguments(args, options))
                {

                    if (args.Length <= 1)
                        Console.Write(options.GetUsage());



                    if (options.Run != null && ValidateRunOptions(options.Run))
                    {


                        var replayer = new PIReplayer();
                        replayer.Start();

                        _logger.Info("Press any key to quit");
                        Console.ReadKey();
                    }

                    if (options.deleteHistory != null && ValidateDeleteHistoryOptions(options.deleteHistory))
                    {
                        // getting the tags
                        var piConnection = new PIConnection(options.deleteHistory[0]);

                        piConnection.Connect();

                        var pointsProvider = new PIPointsProvider(options.deleteHistory[3], piConnection.GetPiServer());

                        foreach (var piPoint in pointsProvider.Points)
                        {
                            var st = AFTime.Parse(options.deleteHistory[1]);
                            var et = AFTime.Parse(options.deleteHistory[2]);
                            _logger.InfoFormat("Deleting history for tag: {0} between {1:G} and {2:G}", piPoint.Name,st.LocalTime,et.LocalTime);

                            piPoint.ReplaceValues(new AFTimeRange(st, et), new List<AFValue>());
                        }



                    }




                }

                else
                {
                    options.GetUsage();
                }

            }
            catch (Exception ex)
            {
                _logger.Error(ex);
               
            }

        }
    }
}