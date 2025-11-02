using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartStorageBackend.Hubs;
using SmartStorageBackend.DTOs;
using SmartStorageBackend.Models;
using Microsoft.EntityFrameworkCore;

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
            robot.Status = "active";

            var savedScanIds = new List<int>();
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
                await _db.SaveChangesAsync();
                savedScanIds.Add(entry.Id);
            }

            // WEB SOCKET - отправка обновления робота (полный объект Robot с правильными полями)
            await _hub.Clients.All.SendAsync("robot_update", new
            {
                id = robot.Id,
                status = robot.Status,
                batteryLevel = robot.BatteryLevel,
                lastUpdate = robot.LastUpdate,
                currentZone = robot.CurrentZone,
                currentRow = robot.CurrentRow,
                currentShelf = robot.CurrentShelf
            });

            // WEB SOCKET - отправка каждого сканирования через scan_update
            foreach (var scan in dto.ScanResults)
            {
                var scanEntry = await _db.InventoryHistory
                    .Where(h => h.RobotId == dto.RobotId && h.ProductId == scan.ProductId && savedScanIds.Contains(h.Id))
                    .OrderByDescending(h => h.ScannedAt)
                    .FirstOrDefaultAsync();

                if (scanEntry != null)
                {
                    await _hub.Clients.All.SendAsync("scan_update", new
                    {
                        id = scanEntry.Id,
                        robot_id = scanEntry.RobotId,
                        product_id = scanEntry.ProductId,
                        product_name = scan.ProductName ?? "",
                        quantity = scanEntry.Quantity,
                        zone = scanEntry.Zone,
                        row_number = scanEntry.RowNumber,
                        shelf_number = scanEntry.ShelfNumber,
                        status = scanEntry.Status,
                        scanned_at = scanEntry.ScannedAt.ToString("o"),
                        created_at = scanEntry.CreatedAt.ToString("o")
                    });
                }
            }

            // WEB SOCKET - отправка alert для критических остатков
            if (dto.ScanResults.Any(s => s.Status == "CRITICAL"))
            {
                await _hub.Clients.All.SendAsync("inventory_alert", new
                {
                    robot_id = dto.RobotId,
                    message = "Обнаружен критический остаток!",
                    time = DateTime.UtcNow
                });
            }

            return Ok(new { status = "received", message_id = Guid.NewGuid() });
        }
    }
}
