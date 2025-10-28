using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartStorageBackend;
using SmartStorageBackend.Hubs;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly SmartStorageContext _db;
    private readonly IHubContext<DashboardHub> _hub;

    public DashboardController(SmartStorageContext db, IHubContext<DashboardHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    [HttpPost("alert")]
    public async Task<IActionResult> SendAlert([FromBody] string message)
    {
        await _hub.Clients.All.SendAsync("inventory_alert", new { message, time = DateTime.UtcNow });
        return Ok(new { status = "alert_sent" });
    }

    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentState()
    {
        // Информация о роботах
        var robots = await _db.Robots
            .Select(r => new
            {
                r.Id,
                r.Status,
                r.BatteryLevel,
                r.CurrentZone,
                r.CurrentRow,
                r.CurrentShelf,
                r.LastUpdate
            }).ToListAsync();

        // Информация о последних сканах
        var recentScans = await _db.InventoryHistory
           .OrderByDescending(h => h.ScannedAt)
           .Take(20)
           .Select(h => new
           {
               h.Id,
               h.RobotId,
               h.ProductId,
               h.Quantity,
               h.Zone,
               h.RowNumber,
               h.ShelfNumber,
               h.Status,
               h.ScannedAt



           })
           .ToListAsync();

        // Статистика
        var stats = new
        {
            total_products = await _db.Products.CountAsync(),
            total_scans = await _db.InventoryHistory.CountAsync(),
            critical_products = await _db.InventoryHistory
                .Where(h => h.Status == "CRITICAL")
                .Select(h => h.ProductId)
                .Distinct()
                .CountAsync(),
            active_robots = robots.Count(r => r.Status == "active"),
            last_update = DateTime.UtcNow
        };

        return Ok(new
        {
            robots,
            recentScans,
            stats
        }
        );
    }
}
