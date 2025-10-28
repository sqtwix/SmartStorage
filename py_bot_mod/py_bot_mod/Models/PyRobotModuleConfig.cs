using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using py_bot_mod.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace py_bot_mod.Models
{
    public static class PyRobotModuleConfig
    {
        public static IServiceCollection AddPyModule(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddScoped<IPyRobotService, PyRobotService>();
            services.AddHttpClient<IPyRobotService, PyRobotService>(client => 
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri("http://localhost:5000");
            });

            return services;
        }
        public static IApplicationBuilder UsePyRobotModule(this IApplicationBuilder app) 
        {
            return app;
        }
    }
}
