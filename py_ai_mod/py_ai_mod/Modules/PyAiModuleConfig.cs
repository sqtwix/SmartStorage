using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using py_ai_mod.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace py_ai_mod.Modules
{
    public static class PyAiModuleConfig
    {
        public static IServiceCollection AddPyAiModule(this IServiceCollection services, IConfiguration configuration)
        {
            // Регистрируем сервис
            services.AddScoped<IPyAiService, PyAiService>();

            // Настраиваем HttpClient для Python API
            services.AddHttpClient<IPyAiService, PyAiService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri("http://localhost:5000");
            });

            return services;
        }

        public static IApplicationBuilder UsePyAiModule(this IApplicationBuilder app)
        {
            // Middleware или конфигурация pipeline если нужно
            return app;
        }
    }
}
