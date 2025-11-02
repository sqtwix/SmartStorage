using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace py_bot_mod.Models
{
    internal class PyRobotModule
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Python robot model";
        public string Description { get; set; } = "Module for Python robot intgration";
        public bool isEnabled { get; set; } = true;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;

        public string PythonApiUrl { get; set; } = string.Empty;
        public string ModelType { get; set; } = "default";
        public int TimeoutSeconds { get; set; } = 30;
    }
}