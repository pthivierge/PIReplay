using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using PIReplayLib;

namespace PIReplay
{
    public partial class PIReplayService : ServiceBase
    {
        private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(PIReplayService));

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
            PIReplayer replayer = new PIReplayer();
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
