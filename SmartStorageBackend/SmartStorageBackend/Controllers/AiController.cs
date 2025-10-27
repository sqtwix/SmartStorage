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

        [HttpPost]
        public async Task<IActionResult> Predict([FromBody] AiPredictRequest request)
        {
            var responce = await _http.PostAsJsonAsync("http://localhost:8000/predict", request);
            if (!responce.IsSuccessStatusCode) {
                return StatusCode(500, "Ai сервис не работает!");
            }

            var result = await responce.Content.ReadFromJsonAsync<AiPredictResponse>();

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
            return Ok(responce);
        }

    }
}
