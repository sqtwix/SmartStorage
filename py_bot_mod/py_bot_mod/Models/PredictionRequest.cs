using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace py_bot_mod.Models
{
    public class PredictionRequest
    {
        public string RobotId { get; set; }
        public string Command { get; set; }
        public object Parameters { get; set; }
    }
}
