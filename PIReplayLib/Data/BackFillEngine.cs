using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSIsoft.AF.PI;

namespace PIReplayLib.Data
{
    public class BackFillEngine
    {
        IEnumerable<PIPoint> _points=null;

        public BackFillEngine(IEnumerable<PIPoint> points)
        {
            _points = points;
        }


        public void Run()
        {
            // find the snapshot at which to start the backfill
            foreach (var pt in _points)
            {
                var snapshotTime = pt.CurrentValue().Timestamp.LocalTime;
                
            }


        }


    }
}
