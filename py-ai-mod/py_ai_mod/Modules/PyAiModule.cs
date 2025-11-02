using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using py_ai_mod.Services;

namespace py_ai_mod.Models
{
    public class PyAiModule 
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Python AI Module";
        public string Description { get; set; } = "Module for Python AI integration";
        public bool IsEnabled { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Конфигурация AI модели
        public string PythonApiUrl { get; set; } = "http://localhost:5000/api/predict";
        public string ModelType { get; set; } = "default";
        public int TimeoutSeconds { get; set; } = 30;
    }
}
