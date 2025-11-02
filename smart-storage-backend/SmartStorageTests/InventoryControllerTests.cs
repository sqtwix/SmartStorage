using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartStorageBackend;
using SmartStorageBackend.Controllers;
using SmartStorageBackend.Models;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SmartStorageTests
{
    public class InventoryControllerTests
    {
        private SmartStorageContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<SmartStorageContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new SmartStorageContext(options);
            db.Products.Add(new Product
            {
                Id = "TEL-4567",
                Name = "Роутер RT-AC68U",
                Category = "Network",
                Min_stock = 10,
                Optimal_stock = 100
            });
            db.SaveChanges();
            return db;
        }

        private IFormFile CreateFile(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "test.csv");
        }

        [Fact]
        public async Task ImportCSV_ValidFile_ShouldReturnOk()
        {
            // Arrange
            var db = GetDbContext();
            var controller = new InventoryController(db);

            var csvData = "product_id;product_name;quantity;zone;date;row;shelf\n" +
                          "TEL-4567;Роутер RT-AC68U;45;A;2024-03-15;12;3\n" +
                          "TEL-8901;Модем DSL-2640U;12;B;2024-03-15;5;2";

            var file = CreateFile(csvData);

            // Act
            var result = await controller.ImportCSV(file);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var parsed = JsonSerializer.Deserialize<JsonElement>(json);
            Assert.Equal(2, parsed.GetProperty("success").GetInt32());
            Assert.Equal(0, parsed.GetProperty("failed").GetInt32());
        }

        [Fact]
        public async Task ImportCSV_InvalidHeader_ShouldReturnBadRequest()
        {
            // Arrange
            var db = GetDbContext();
            var controller = new InventoryController(db);

            // Ошибочный CSV — нет заголовков или другие имена столбцов
            var csvData = "id;name;qty\n123;Ошибка;10";
            var file = CreateFile(csvData);

            // Act
            var result = await controller.ImportCSV(file);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            var message = badResult.Value?.ToString();
            Assert.Contains("Неверный формат CSV", message);
        }

        [Fact]
        public async Task GetHistory_ShouldReturnFilteredData()
        {
            // Arrange
            var db = GetDbContext();
            db.InventoryHistory.AddRange(
                new InventoryHistory
                {
                    ProductId = "TEL-4567",
                    RobotId = "R1",
                    Quantity = 5,
                    Zone = "A",
                    Status = "CRITICAL",
                    ScannedAt = DateTime.UtcNow.AddHours(-2),
                    CreatedAt = DateTime.UtcNow
                },
                new InventoryHistory
                {
                    ProductId = "TEL-8901",
                    RobotId = "R2",
                    Quantity = 30,
                    Zone = "B",
                    Status = "OK",
                    ScannedAt = DateTime.UtcNow.AddHours(-1),
                    CreatedAt = DateTime.UtcNow
                });
            db.SaveChanges();

            var controller = new InventoryController(db);

            // Act
            var result = await controller.GetHistory(
                from: DateTime.UtcNow.AddDays(-1),
                to: DateTime.UtcNow,
                zone: "A",
                status: "CRITICAL",
                page: 1,
                pageSize: 10);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(ok.Value);
            var parsed = JsonSerializer.Deserialize<JsonElement>(json);
            Assert.Equal(1, parsed.GetProperty("total").GetInt32()); // Только одна запись подходит
        }
    }
}
