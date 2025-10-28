using py_bot_mod.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace py_bot_mod.Services
{
    public interface IPyRobotService
    {
        Task<PredictionResponse> PredictAsync(PredictionRequest request);
        Task<List<RobotData>> GetRobotsDataAsync();
        Task<RobotData> GetRobotDataAsync(string robotId);
        Task<string> SendCommandAsync(string robotId, string command);
    }
}
