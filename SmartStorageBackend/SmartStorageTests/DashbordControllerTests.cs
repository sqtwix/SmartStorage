using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Json;
using SmartStorageBackend;
using SmartStorageBackend.Controllers;
using SmartStorageBackend.Hubs;
using SmartStorageBackend.Models;
using Xunit;

namespace SmartStorageTests
{
    public class DashboardControllerTests
    {
        private SmartStorageContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<SmartStorageContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var db = new SmartStorageContext(options);

            // Добавляем тестовые данные
            db.Robots.Add(new Robot
            {
                Id = "R1",
                Status = "active",
                BatteryLevel = 85,
                CurrentZone = "A",
                CurrentRow = 1,
                CurrentShelf = 2
            });

            db.Products.Add(new Product
            {
                Id = "P1",
                Name = "Screwdriver",
                Category = "Tools",
                Min_stock = 5,
                Optimal_stock = 20
            });

            db.InventoryHistory.Add(new InventoryHistory
            {
                RobotId = "R1",
                ProductId = "P1",
                Quantity = 10,
                Zone = "A",
                RowNumber = 1,
                ShelfNumber = 2,
                Status = "OK",
                ScannedAt = DateTime.UtcNow.AddMinutes(-30),
                CreatedAt = DateTime.UtcNow
            });

            db.SaveChanges();
            return db;
        }

        [Fact]
        public async Task GetCurrentState_ShouldReturnExpectedData()
        {
            // Arrange
            var db = GetInMemoryDb();

            // Мокаем HubContext (SignalR)
            var hubMock = new Mock<IHubContext<DashboardHub>>();
            var controller = new DashboardController(db, hubMock.Object);

            // Act
            var result = await controller.GetCurrentState();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;

            // Проверяем наличие ключей
            Assert.True(root.TryGetProperty("robots", out _));
            Assert.True(root.TryGetProperty("recentScans", out _));
            Assert.True(root.TryGetProperty("stats", out _));

            // Проверяем что robots не пустой
            var robotsArray = root.GetProperty("robots");
            Assert.True(robotsArray.GetArrayLength() > 0);

            // Проверяем статистику
            var stats = root.GetProperty("stats");
            Assert.True(stats.GetProperty("total_products").GetInt32() > 0);
        }
    }
}
