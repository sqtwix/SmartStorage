using Microsoft.AspNetCore.Mvc;
using SmartStorageBackend.DTOs;
using SmartStorageBackend.Models;
using System.Net.Http.Json;

namespace SmartStorageBackend.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly SmartStorageContext _db;
        private readonly HttpClient _http;

        public AiController(SmartStorageContext db, IHttpClientFactory httpFactory)
        {
            _db = db;
            _http = httpFactory.CreateClient();
        }

        [HttpPost("predict")]
        public async Task<IActionResult> Predict([FromBody] AiPredictRequest request)
        {
            // В Docker используем имя сервиса, в локальной разработке - localhost
            var aiModuleUrl = Environment.GetEnvironmentVariable("AI_MODULE_URL") ?? "http://ai-module:8000";
            var responce = await _http.PostAsJsonAsync($"{aiModuleUrl}/predict", request);

            if (!responce.IsSuccessStatusCode) {
                return StatusCode(500, "Ai ������ �� ��������!");
            }

            var result = await responce.Content.ReadFromJsonAsync<AiPredictResponse>();

            if (result == null)
            {
                return StatusCode(500, "������ ��� ��������� ������ �� AI �������");
            }

            foreach (var p in result!.Predictions)
            {
                _db.AiPredictions.Add(new AiPrediction
                {
                    ProductId = p.ProductId,
                    PredictionDate = p.PredictionDate,
                    DaysUntilStockout = p.DaysUntilStockout,
                    RecommendedOrder = p.RecommendedOrder,
                    ConfidenceScore = p.ConfidenceScore
                });
            }

            await _db.SaveChangesAsync();
            return Ok(result);
        }

    }
}
