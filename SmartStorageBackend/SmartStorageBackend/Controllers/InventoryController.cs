using Microsoft.AspNetCore.Mvc;
using System.Data;
using SmartStorageBackend.DTOs;
using SmartStorageBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CsvHelper;

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
            var fromDate = from ?? DateTime.UtcNow.AddDays(-1);
            var toDate = to ?? DateTime.UtcNow;

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
        [RequestSizeLimit(15_000_000)] // �������� 15 ��
        public async Task<IActionResult> ImportCSV(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "���� �� ��������." });
            }

            //  ������������� ������ ��� ������
            var successCount = 0;
            var failedCount = 0;
            var errors = new List<string>();

            // ���������� �����
            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                try
                {
                    var records = csv.GetRecords<InventoryCsvRow>().ToList();

                    foreach (var record in records)
                    {
                        try
                        {
                            // ���������, ���������� �� �������
                            var productExists = _db.Products.Any(p => p.Id == record.ProductId);
                            if (!productExists)
                            {
                                errors.Add($"������� {record.ProductId} �� ������");
                                failedCount++;
                                continue;
                            }

                            // ��������� ����� ������ �������
                            var entry = new InventoryHistory
                            {
                                ProductId = record.ProductId,
                                RobotId = record.RobotId ?? "manual_import",
                                Quantity = record.Quantity,
                                Zone = record.Zone,
                                RowNumber = record.RowNumber,
                                ShelfNumber = record.ShelfNumber,
                                Status = record.Status,
                                ScannedAt = DateTime.UtcNow,
                                CreatedAt = DateTime.UtcNow
                            };

                            _db.InventoryHistory.Add(entry);
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failedCount++;
                            errors.Add($"������ � ������ {record.ProductId}: {ex.Message}");
                        }
                    }

                    await _db.SaveChangesAsync();
                }
                catch (HeaderValidationException)
                {
                    return BadRequest(new { message = "������������ ������ CSV-�����." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = $"������ ��������� CSV: {ex.Message}" });
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
