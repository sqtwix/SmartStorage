using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SmartStorageBackend.DTOs;
using SmartStorageBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;


namespace SmartStorageBackend.Controllers
{
    [ApiController]
    [Route("api/export")]
    public class ExportExcelController : ControllerBase
    {
        private readonly SmartStorageContext _db;

        public ExportExcelController(SmartStorageContext db)
        {
            _db = db;
        }

        [HttpGet("excel")]
        public async Task<IActionResult> ExportToExcel([FromQuery] string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return BadRequest("Не указаны ID!");
            }

            // Разбираем список id
            var idList = ids.Split(',').Select(id => int.TryParse(id, out var val) ? val : (int?)null)
                            .Where(v => v.HasValue)
                            .Select(v => v!.Value)
                            .ToList();

            if (idList.Count == 0)
                return BadRequest(new { message = "Некорректные ID записей" });

            // Получаем записи из базы данных
            var records =  _db.InventoryHistory
                .Where(h => idList.Contains(h.Id))
                .OrderBy(h => h.ScannedAt)
                .Select (h => new
                {
                    h.Id,
                    h.RobotId,
                    h.ProductId,
                    ProductName = h.Product != null ? h.Product.Name : "N/A",
                    h.Quantity,
                    h.Zone,
                    h.RowNumber,
                    h.ShelfNumber,
                    h.Status,
                    h.ScannedAt,
                    h.CreatedAt
                }).ToList();

                if (records.Count == 0)
                    return NotFound(new { message = "Записи не найдены" });

                // Создаем Excel файл
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("InventoryExport");

                // Заголовки столбцов
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Robot ID";
                worksheet.Cells[1, 3].Value = "Product ID";
                worksheet.Cells[1, 4].Value = "Product Name";
                worksheet.Cells[1, 5].Value = "Quantity";
                worksheet.Cells[1, 6].Value = "Zone";
                worksheet.Cells[1, 7].Value = "Row Number";
                worksheet.Cells[1, 8].Value = "Shelf Number";
                worksheet.Cells[1, 9].Value = "Status";
                worksheet.Cells[1, 10].Value = "Scanned At";
                worksheet.Cells[1, 11].Value = "Created At";

                // Заполнение строк
                int row = 2;
                foreach (var r in records)
                {
                    worksheet.Cells[row, 1].Value = r.Id;
                    worksheet.Cells[row, 2].Value = r.RobotId;
                    worksheet.Cells[row, 3].Value = r.ProductId;
                    worksheet.Cells[row, 4].Value = r.ProductName;
                    worksheet.Cells[row, 5].Value = r.Quantity;
                    worksheet.Cells[row, 6].Value = r.Zone;
                    worksheet.Cells[row, 7].Value = r.RowNumber;
                    worksheet.Cells[row, 8].Value = r.ShelfNumber;
                    worksheet.Cells[row, 9].Value = r.Status;
                    worksheet.Cells[row, 10].Value = r.ScannedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    worksheet.Cells[row, 11].Value = r.CreatedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                // Готовим файл
                var stream = new MemoryStream(package.GetAsByteArray());
                var filename = $"inventory_export_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx";

            return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    filename
                );
        }
    }
}
