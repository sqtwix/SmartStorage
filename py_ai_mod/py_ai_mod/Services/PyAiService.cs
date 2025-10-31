using Microsoft.Extensions.Logging;
using py_ai_mod.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace py_ai_mod.Services
{
    internal class PyAiService : IPyAiService
    {

        private readonly HttpClient _httpClient;
        private readonly ILogger<PyAiService> _logger;
        private readonly PyAiModule _moduleConfig;

        public PyAiService(HttpClient httpClient, ILogger<PyAiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _moduleConfig = new PyAiModule();
        }

        public async Task<string> PredictAsync(string inputText)
        {
            try
            {
                _logger.LogInformation("Sending prediction request for text: {Text}", inputText);

                var requestData = new
                {
                    text = inputText,
                    model = _moduleConfig.ModelType
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_moduleConfig.PythonApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Prediction successful: {Result}", result);
                    return result;
                }
                else
                {
                    _logger.LogError("Python API error: {StatusCode}", response.StatusCode);
                    return $"Error: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Python AI API");
                return $"Exception: {ex.Message}";
            }
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_moduleConfig.PythonApiUrl.Replace("/predict", "/health"));
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public Task<PyAiModule> GetModuleInfoAsync()
        {
            return Task.FromResult(_moduleConfig);
        }
    }
}

