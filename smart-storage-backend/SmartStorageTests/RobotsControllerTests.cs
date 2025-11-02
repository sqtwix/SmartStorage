using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartStorageBackend;
using SmartStorageBackend.Controllers;
using SmartStorageBackend.DTOs;
using SmartStorageBackend.Hubs;
using SmartStorageBackend.Models;
using Xunit;

namespace SmartStorageTests
{
    public class RobotsControllerTests
    {
        private SmartStorageContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<SmartStorageContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var db = new SmartStorageContext(options);
            return db;
        }

        [Fact]
        public async Task ReceiveData_ShouldAddRobotAndInventoryHistory()
        {
            // Arrange
            var db = GetInMemoryDb();
            var hubMock = new Mock<IHubContext<DashboardHub>>();
            var clientsMock = new Mock<IHubClients>();
            var allMock = new Mock<IClientProxy>();

            hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.All).Returns(allMock.Object);

            var controller = new RobotsController(db, hubMock.Object);

            var dto = new RobotDataDTO
            {
                RobotId = "RB-01",
                BatteryLevel = 90,
                Timestamp = DateTime.UtcNow,
                Location = new RobotLocationDTO { Zone = "A", Row = 1, Shelf = 2 },
                ScanResults = new List<ScanResultDTO>
                {
                    new() { ProductId = "P-101", Quantity = 15, Status = "OK" },
                    new() { ProductId = "P-102", Quantity = 3, Status = "LOW_STOCK" }
                }
            };

            // Act
            var result = await controller.ReceiveData(dto);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;

            // Assert
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            Assert.Contains("received", json);
            Assert.Contains("message_id", json);
            Assert.Single(db.Robots);
            Assert.Equal(2, db.InventoryHistory.Count());
        }

        [Fact]
        public async Task ReceiveData_ShouldUpdateExistingRobot()
        {
            var db = GetInMemoryDb();
            db.Robots.Add(new Robot
            {
                Id = "RB-01",
                BatteryLevel = 70,
                CurrentZone = "A",
                CurrentRow = 1,
                CurrentShelf = 1,
                LastUpdate = DateTime.UtcNow.AddMinutes(-10)
            });
            await db.SaveChangesAsync();

            var hubMock = new Mock<IHubContext<DashboardHub>>();
            var clientsMock = new Mock<IHubClients>();
            var allMock = new Mock<IClientProxy>();
            hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.All).Returns(allMock.Object);

            var controller = new RobotsController(db, hubMock.Object);

            var dto = new RobotDataDTO
            {
                RobotId = "RB-01",
                BatteryLevel = 60,
                Timestamp = DateTime.UtcNow,
                Location = new RobotLocationDTO { Zone = "B", Row = 3, Shelf = 2 },
                ScanResults = new List<ScanResultDTO>()
            };

            var result = await controller.ReceiveData(dto);
            var okResult = Assert.IsType<OkObjectResult>(result);

            var updatedRobot = db.Robots.First();
            Assert.Equal("B", updatedRobot.CurrentZone);
            Assert.Equal(60, updatedRobot.BatteryLevel);
        }
    }
}