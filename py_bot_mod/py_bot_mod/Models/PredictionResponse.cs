using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace py_bot_mod.Models
{
    public class PredictionResponse
    {
        public string RobotId { get; set; }
        public string Status { get; set; }
        public object Data { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
