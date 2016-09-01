﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using OSIsoft.AF;
using OSIsoft.AF.Asset;

namespace PIReplayLib
{
    public class Replayer
    {

        private static readonly ILog _logger = LogManager.GetLogger(typeof(Replayer));
        private static Timer timer = null;

        private DataReader _dataReader = null;
        private DataWriter _dataWriter = null;

        private BlockingCollection<List<AFValue>> _queue=new BlockingCollection<List<AFValue>>();


        public Replayer()
        {
            
        }

        public void RunFromCommandLine(string server, string pointsQuery)
        {
            this.Run(server, pointsQuery);
           _logger.Info("Press a key to stop the application.");
           Console.ReadKey();
            Stop();

        }

        public void Run(string server, string pointsQuery)
        {
            var connection=new PIConnection(server);
            var pointsProvier=new PIPointsProvider(pointsQuery,connection.GetPiServer());

            // here we need our data readers and writers
            _dataReader = new DataReader(pointsProvier, _queue);
            _dataWriter = new DataWriter(_queue, connection.GetPiServer());
            _dataWriter.Run();
            
            _dataReader.RunBackfill();

            _logger.Info("Starting the normal operations process");
            _dataReader.Run(AppSettings.Default.DataCollectionFrequencySeconds);

        }

        public void Stop()
        {
            _dataReader.Stop();
            _dataWriter.Stop();
            _logger.Info("Application Stopped");
        }


    }
}
