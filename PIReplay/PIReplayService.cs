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
using System.ServiceProcess;
using log4net;
using PIReplayLib;

namespace PIReplay
{
    public partial class PIReplayService : ServiceBase
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (PIReplayService));

        public PIReplayService()
        {
            InitializeComponent();
        }

        public void ConsoleStart(string[] args)
        {
            OnStart(args);
            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }

        protected override void OnStart(string[] args)
        {
            _logger.Info("Service starting...");
            var replayer = new PIReplayer();
            replayer.Start();
            _logger.Info("Service started...");
        }

        public void ConsoleStop()
        {
            OnStop();
        }

        protected override void OnStop()
        {
        }
    }
}