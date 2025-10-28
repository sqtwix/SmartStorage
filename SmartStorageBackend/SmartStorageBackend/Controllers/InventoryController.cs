using Microsoft.AspNetCore.Mvc;
using System.Data;
using SmartStorageBackend.DTOs;
using SmartStorageBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

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

        [HttpPost("import")]
        [RequestSizeLimit(15_000_000)] // Максимальный размер файла - 15 мб
       public async Task<IActionResult> ImportCSV(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Файл не был загружен." });

            int successCount = 0;
            int failedCount = 0;
            var errors = new List<string>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";", // Разделитель
                HasHeaderRecord = true,
                MissingFieldFound = null, // игнорируем пустые поля
                BadDataFound = null
            };

            // Чтение файла
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, config))
            {
                try
                {
                    csv.Context.RegisterClassMap<InventoryCsvRowMap>(); 
                    var records = csv.GetRecords<InventoryCsvRow>().ToList();

                    foreach (var record in records)
                    {
                        try
                        {
                            // Проверка корректности данных
                            if (string.IsNullOrEmpty(record.ProductId) ||
                                string.IsNullOrEmpty(record.ProductName) ||
                                record.Quantity < 0 ||
                                string.IsNullOrEmpty(record.Zone))
                            {
                                failedCount++;
                                errors.Add($"Некорректные данные: {record.ProductId} / {record.ProductName}");
                                continue;
                            }

                            // Проверка существования продукта
                            var product = await _db.Products.FindAsync(record.ProductId);
                            if (product == null)
                            {
                                // Если нет — создаём новый
                                product = new Product
                                {
                                    Id = record.ProductId,
                                    Name = record.ProductName,
                                    Category = "Imported",
                                    Min_stock = 10,
                                    Optimal_stock = 100
                                };
                                _db.Products.Add(product);
                            }

                            // Добавляем запись в историю
                            var history = new InventoryHistory
                            {
                                ProductId = record.ProductId,
                                Quantity = record.Quantity,
                                Zone = record.Zone,
                                RowNumber = record.Row,
                                ShelfNumber = record.Shelf,
                                Status = record.Quantity == 0 ? "CRITICAL"
                                        : record.Quantity < 10 ? "LOW_STOCK"
                                        : "OK",
                                ScannedAt = record.Date,
                                CreatedAt = DateTime.UtcNow,
                                RobotId = "manual_import"
                            };

                            _db.InventoryHistory.Add(history);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            errors.Add($"Ошибка при обработке строки {record.ProductId}: {ex.Message}");
                        }
                    }

                    await _db.SaveChangesAsync();
                }
                catch (HeaderValidationException)
                {
                    return BadRequest(new
                    {
                        message = "Неверный формат CSV. Ожидается заголовок: product_id;product_name;quantity;zone;date;row;shelf"
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = $"Ошибка при чтении файла: {ex.Message}" });
                }
            }

            return Ok(new
            {
                success = successCount,
                failed = failedCount,
                errors
            });
        }
    }
}
