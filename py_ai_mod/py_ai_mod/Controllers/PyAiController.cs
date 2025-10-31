using Microsoft.AspNetCore.Mvc;
using py_ai_mod.Models;
using py_ai_mod.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace py_ai_mod.Controllers
{
    [ApiController]
    [Route("api/ai/py-ai-mod")]
    public class PyAiController : ControllerBase
    {
        private readonly IPyAiService _aiService;

        public PyAiController(IPyAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("py_ai_mod is okay");
        }

        [HttpGet("health")]
        public async Task<IActionResult> GetHealth()
        {
            var isHealthy = await _aiService.IsHealthyAsync();
            return Ok(new { healthy = isHealthy });
        }

        [HttpGet("info")]
        public async Task<ActionResult<PyAiModule>> GetInfo()
        {
            var info = await _aiService.GetModuleInfoAsync();
            return Ok(info);
        }

        [HttpPost("predict")]
        public async Task<IActionResult> Predict([FromBody] PredictionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Text is required");
            }

            var result = await _aiService.PredictAsync(request.Text);
            return Ok(new { prediction = result });
        }
    }

    public class PredictionRequest
    {
        public string Text { get; set; } = string.Empty;
    }
}
