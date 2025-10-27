using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartStorageBackend.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly SmartStorageContext _db;
        
        public InventoryController(SmartStorageContext db)
        {
            _db = db;
        }


        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? zone,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Установление периода (по умолчанию 24 ч)
            var fromDate = from ?? DateTime.UtcNow.AddDays(-1);
            var toDate = to ?? DateTime.UtcNow;

            // Базовый запрос
            var query = _db.InventoryHistory
                .Where(h => h.ScannedAt >= fromDate && h.ScannedAt <= toDate)
                .AsQueryable();

            // Фильтрация
            if (!string.IsNullOrEmpty(zone))
                query = query.Where(h => h.Zone.ToLower() == zone.ToLower());

            if (!string.IsNullOrEmpty(status))
                query = query.Where(h => h.Status.ToLower() == status.ToLower());

            // Получаем общее кол-во товаров
            var total = query.Count();

            // Получаем нужную страницу
            var items = await query
                .OrderByDescending(h => h.ScannedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                    h.ScannedAt,
                    h.CreatedAt
                }).ToListAsync();

            var responce = new
            {
                total,
                items,
                pagination = new
                {
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)total / pageSize)
                }
            };

            return Ok(responce);
        }
    }
}
