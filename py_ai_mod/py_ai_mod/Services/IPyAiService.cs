using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using py_ai_mod.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace py_ai_mod.Services
{
    public interface IPyAiService
    {
        Task<string> PredictAsync(string inputText);
        Task<bool> IsHealthyAsync();
        Task<PyAiModule> GetModuleInfoAsync();
    }
}
