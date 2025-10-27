using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartStorageBackend.Hubs;
using SmartStorageBackend.DTOs;
using SmartStorageBackend.Models;

namespace SmartStorageBackend.Controllers
{
    [ApiController]
    [Route("api/robots")]
    public class RobotsController : ControllerBase
    {
        private readonly SmartStorageContext _db;
        private readonly IHubContext<DashboardHub> _hub;

        public RobotsController(SmartStorageContext db, IHubContext<DashboardHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        [Authorize(Roles = "robot")]
        [HttpPost("data")]
        public async Task<IActionResult> ReceiveData([FromBody] RobotDataDTO dto)
        {
            var robot = await _db.Robots.FindAsync(dto.RobotId);
            if (robot == null)
            {
                robot = new Robot { Id = dto.RobotId, Status = "active" };
                _db.Robots.Add(robot);
            }

            robot.BatteryLevel = dto.BatteryLevel;
            robot.CurrentZone = dto.Location.Zone;
            robot.CurrentRow = dto.Location.Row;
            robot.CurrentShelf = dto.Location.Shelf;
            robot.LastUpdate = dto.Timestamp;

            foreach (var scan in dto.ScanResults)
            {
                var entry = new InventoryHistory
                {
                    RobotId = dto.RobotId,
                    ProductId = scan.ProductId,
                    Quantity = scan.Quantity,
                    Zone = dto.Location.Zone,
                    RowNumber = dto.Location.Row,
                    ShelfNumber = dto.Location.Shelf,
                    Status = scan.Status,
                    ScannedAt = dto.Timestamp
                };
                _db.InventoryHistory.Add(entry);
            }

            await _db.SaveChangesAsync();

            // WEB SOCKET - Отправляем обновления всем подключённым клиентам
            await _hub.Clients.All.SendAsync("robot_update", new
            {
                dto.RobotId,
                dto.Location,
                dto.BatteryLevel,
                dto.Timestamp
            });

            return Ok(new { status = "received", message_id = Guid.NewGuid() });
        }
    }
}
