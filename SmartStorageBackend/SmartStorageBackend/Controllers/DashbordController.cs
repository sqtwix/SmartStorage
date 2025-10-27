using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
}
