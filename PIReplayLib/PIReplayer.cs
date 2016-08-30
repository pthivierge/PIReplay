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
using System.Configuration;
using log4net;
using OSIsoft.AF.PI;

namespace PIReplayLib
{
    /// <summary>
    ///     Coordinates the PIReader and PIWriter.
    /// </summary>
    public class PIReplayer
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof (PIReplayer));
        private readonly PIPointList _destPoints;

        private readonly PIServer _destServer;

        private readonly PIReader _reader;
        private readonly PIPointList _sourcePoints;
        private readonly PIServer _sourceServer;
        private readonly PIWriter _writer;

        /// <summary>
        ///     Connect to source and destination PI Data Archives. Find the source and destination PI Points.
        ///     Instantiate the PIReader and PIWriter.
        /// </summary>
        public PIReplayer()
        {
            try
            {
                _sourceServer = new PIServers()[ConfigurationManager.AppSettings["sourceServer"]];
                _sourceServer.Connect();

                _destServer = new PIServers()[ConfigurationManager.AppSettings["destServer"]];
                _destServer.Connect();
            }
            catch (PIConnectionException ex)
            {
                _logger.Info(ex.ToString());
            }

            _logger.Info("Loading points");

            _sourcePoints =
                new PIPointList(PIPoint.FindPIPoints(_sourceServer, ConfigurationManager.AppSettings["sourceNameFilter"],
                    ConfigurationManager.AppSettings["sourcePS"])
                    );
            _destPoints =
                new PIPointList(PIPoint.FindPIPoints(_destServer, ConfigurationManager.AppSettings["destNameFilter"],
                    ConfigurationManager.AppSettings["destPS"])
                    );

            _logger.Info(string.Format("Done loading {0} points", _sourcePoints.Count));

            // The PIReader passes the data to the PIWriter via the DataQueue.
            var queue = new DataQueue();
            _reader = new PIReader(this, _sourceServer, _sourcePoints, _destServer, _destPoints, queue);
            _writer = new PIWriter(this, _sourceServer, _sourcePoints, _destServer, _destPoints, queue);
        }

        public void Start()
        {
            _writer.Start();

            _logger.Info("Started reader and writer");
        }

        public void RequestFill(bool initial = false)
        {
            _reader.GetPages(initial);
        }
    }
}