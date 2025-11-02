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
            // ������������ ������� (�� ��������� 24 �)
            var fromDate = DateTime.SpecifyKind(from ?? DateTime.UtcNow.AddDays(-1), DateTimeKind.Utc);
            var toDate = DateTime.SpecifyKind(to ?? DateTime.UtcNow, DateTimeKind.Utc);

            // ������� ������
            var query = _db.InventoryHistory
                .Where(h => h.ScannedAt >= fromDate && h.ScannedAt <= toDate)
                .AsQueryable();

            // ����������
            if (!string.IsNullOrEmpty(zone))
                query = query.Where(h => h.Zone.ToLower() == zone.ToLower());

            if (!string.IsNullOrEmpty(status))
                query = query.Where(h => h.Status.ToLower() == status.ToLower());

            // �������� ����� ���-�� �������
            var total = query.Count();

            // �������� ������ ��������
            var items = await query
                .OrderByDescending(h => h.ScannedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Join(
                    _db.Products,
                    h => h.ProductId,
                    p => p.Id,
                    (h, p) => new
                    {
                        h.Id,
                        RobotId = h.RobotId,
                        ProductId = h.ProductId,
                        ProductName = p.Name,
                        Zone = h.Zone,
                        Status = h.Status,
                        Date = h.ScannedAt,
                        ExpectedQuantity = p.Optimal_stock,
                        ActualQuantity = h.Quantity
                    }
                ).ToListAsync();

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
        [RequestSizeLimit(15_000_000)] // ������������ ������ ����� - 15 ��
       public async Task<IActionResult> ImportCSV(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "���� �� ��� ��������." });

            int successCount = 0;
            int failedCount = 0;
            var errors = new List<string>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";", // �����������
                HasHeaderRecord = true,
                MissingFieldFound = null, // ���������� ������ ����
                BadDataFound = null
            };

            // ������ �����
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
                            // �������� ������������ ������
                            if (string.IsNullOrEmpty(record.ProductId) ||
                                string.IsNullOrEmpty(record.ProductName) ||
                                record.Quantity < 0 ||
                                string.IsNullOrEmpty(record.Zone))
                            {
                                failedCount++;
                                errors.Add($"������������ ������: {record.ProductId} / {record.ProductName}");
                                continue;
                            }

                            // �������� ������������� ��������
                            var product = await _db.Products.FindAsync(record.ProductId);
                            if (product == null)
                            {
                                // ���� ��� � ������ �����
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

                            var dateUtc = DateTime.SpecifyKind(record.Date, DateTimeKind.Utc); // ���������� Kind

                            // ��������� ������ � �������
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
                                ScannedAt = dateUtc,
                                CreatedAt = DateTime.UtcNow,
                                RobotId = "manual_import"
                            };

                            _db.InventoryHistory.Add(history);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            errors.Add($"������ ��� ��������� ������ {record.ProductId}: {ex.Message}");
                        }
                    }

                    await _db.SaveChangesAsync();
                }
                catch (HeaderValidationException)
                {
                    return BadRequest(new
                    {
                        message = "�������� ������ CSV. ��������� ���������: product_id;product_name;quantity;zone;date;row;shelf"
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = $"������ ��� ������ �����: {ex.Message}" });
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
