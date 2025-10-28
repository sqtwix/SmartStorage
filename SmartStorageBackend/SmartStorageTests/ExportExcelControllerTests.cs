using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorageBackend;
using SmartStorageBackend.Controllers;
using SmartStorageBackend.Models;
using System.Text;
using Xunit;

namespace SmartStorageTests
{
    public class ExportExcelControllerTests
    {
        private SmartStorageContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<SmartStorageContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new SmartStorageContext(options);

            // Добавляем тестовые данные
            db.Products.Add(new Product
            {
                Id = "TEL-4567",
                Name = "Роутер RT-AC68U",
                Category = "Network"
            });

            db.InventoryHistory.AddRange(
                new InventoryHistory
                {
                    Id = 1,
                    ProductId = "TEL-4567",
                    Quantity = 10,
                    Zone = "A",
                    RowNumber = 12,
                    ShelfNumber = 3,
                    Status = "OK",
                    ScannedAt = DateTime.UtcNow.AddHours(-1),
                    CreatedAt = DateTime.UtcNow,
                    RobotId = "R1"
                },
                new InventoryHistory
                {
                    Id = 2,
                    ProductId = "TEL-4567",
                    Quantity = 5,
                    Zone = "B",
                    RowNumber = 5,
                    ShelfNumber = 2,
                    Status = "LOW_STOCK",
                    ScannedAt = DateTime.UtcNow.AddHours(-2),
                    CreatedAt = DateTime.UtcNow,
                    RobotId = "R2"
                }
            );

            db.SaveChanges();
            return db;
        }

        [Fact]
        public async Task ExportToExcel_ValidIds_ShouldReturnFile()
        {
            // Arrange
            var db = GetDbContext();
            var controller = new ExportExcelController(db);

            // Act
            var result = await controller.ExportToExcel("1,2");

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.NotEmpty(fileResult.FileContents);
            Assert.Contains(".xlsx", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportToExcel_EmptyIds_ShouldReturnBadRequest()
        {
            // Arrange
            var db = GetDbContext();
            var controller = new ExportExcelController(db);

            // Act
            var result = await controller.ExportToExcel("");

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Не указаны ID", badRequest.Value!.ToString());
        }

        [Fact]
        public async Task ExportToExcel_InvalidIds_ShouldReturnBadRequest()
        {
            // Arrange
            var db = GetDbContext();
            var controller = new ExportExcelController(db);

            // Act
            var result = await controller.ExportToExcel("abc,xyz");

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Некорректные ID", badRequest.Value!.ToString());
        }

        [Fact]
        public async Task ExportToExcel_NotExistingIds_ShouldReturnNotFound()
        {
            // Arrange
            var db = GetDbContext();
            var controller = new ExportExcelController(db);

            // Act
            var result = await controller.ExportToExcel("999");

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("Записи не найдены", notFound.Value!.ToString());
        }
    }
}
