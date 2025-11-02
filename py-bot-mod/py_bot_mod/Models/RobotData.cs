using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace py_bot_mod.Models
{
    public class RobotData
    {
        public string RobotId { get; set; }
        public DateTime Timestamp { get; set; }
        public Location Location { get; set; }
        public List<ScanResult> ScanResults { get; set; }
        public double BatteryLevel { get; set; }
        public string NextCheckpoint { get; set; }
    }
}
