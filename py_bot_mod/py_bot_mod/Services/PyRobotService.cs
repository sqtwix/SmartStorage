using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using py_bot_mod.Models;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace py_bot_mod.Services
{
    public class PyRobotService : IPyRobotService
    {
        private readonly HttpClient _httpClient;
        private readonly string _pythonApiUrl;

        public PyRobotService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _pythonApiUrl = configuration["PythonApi:BaseUrl"] ?? "http://localhost:3000";
        }

        public async Task<PredictionResponse> PredictAsync(PredictionRequest request)
        {
            try
            {
                // Ваш скрипт уже отправляет данные на /api/robots/data
                // Мы можем либо:
                // 1. Получать данные которые скрипт уже отправляет
                // 2. Или модифицировать скрипт чтобы он принимал команды

                // Простой вариант - получаем текущие данные робота
                var response = await _httpClient.GetAsync($"{_pythonApiUrl}/api/robots/data");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return new PredictionResponse
                    {
                        RobotId = request.RobotId,
                        Status = "Success",
                        Data = content,
                        Message = "Data retrieved successfully",
                        Timestamp = DateTime.UtcNow
                    };
                }

                return new PredictionResponse
                {
                    RobotId = request.RobotId,
                    Status = "Error",
                    Message = "Failed to get robot data",
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new PredictionResponse
                {
                    RobotId = request.RobotId,
                    Status = "Error",
                    Message = ex.Message,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<List<RobotData>> GetRobotsDataAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_pythonApiUrl}/api/robots/data");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var robotsData = JsonSerializer.Deserialize<List<RobotData>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return robotsData ?? new List<RobotData>();
                }

                return new List<RobotData>();
            }
            catch (Exception)
            {
                return new List<RobotData>();
            }
        }

        public async Task<RobotData> GetRobotDataAsync(string robotId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_pythonApiUrl}/api/robots/data/{robotId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<RobotData>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string> SendCommandAsync(string robotId, string command)
        {
            var request = new PredictionRequest
            {
                RobotId = robotId,
                Command = command,
                Parameters = new { }
            };

            var result = await PredictAsync(request);
            return result.Message;
        }
    }
}
