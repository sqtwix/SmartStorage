using Microsoft.AspNetCore.Mvc;
using py_bot_mod.Models;
using py_bot_mod.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace py_bot_mod.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PyRobotController : ControllerBase
    {
        private readonly IPyRobotService _robotService;

        public PyRobotController(IPyRobotService robotService)
        {
            _robotService = robotService;
        }

        [HttpPost("predict")]
        public async Task<ActionResult<PredictionResponse>> Predict([FromBody] PredictionRequest request)
        {
            var result = await _robotService.PredictAsync(request);
            return Ok(result);
        }

        [HttpGet("data")]
        public async Task<ActionResult<List<RobotData>>> GetAllRobotsData()
        {
            var data = await _robotService.GetRobotsDataAsync();
            return Ok(data);
        }

        [HttpGet("data/{robotId}")]
        public async Task<ActionResult<RobotData>> GetRobotData(string robotId)
        {
            var data = await _robotService.GetRobotDataAsync(robotId);
            if (data == null)
                return NotFound($"Robot {robotId} not found");

            return Ok(data);
        }

        [HttpPost("command/{robotId}")]
        public async Task<ActionResult<string>> SendCommand(string robotId, [FromBody] string command)
        {
            var result = await _robotService.SendCommandAsync(robotId, command);
            return Ok(result);
        }

        [HttpPost("emergency-stop/{robotId}")]
        public async Task<ActionResult<string>> EmergencyStop(string robotId)
        {
            var result = await _robotService.SendCommandAsync(robotId, "emergency_stop");
            return Ok(result);
        }

        [HttpPost("return-to-base/{robotId}")]
        public async Task<ActionResult<string>> ReturnToBase(string robotId)
        {
            var result = await _robotService.SendCommandAsync(robotId, "return_to_base");
            return Ok(result);
        }
    }
}
