using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using SmartStorageBackend;
using SmartStorageBackend.Controllers;
using SmartStorageBackend.DTOs;
using SmartStorageBackend.Models;
using Xunit;

namespace SmartStorageTests
{
    public class AuthControllerTests
    {
        private SmartStorageContext GetDb()
        {
            var options = new DbContextOptionsBuilder<SmartStorageContext>()
         .UseNpgsql("Host=localhost;Port=5432;Database=smart_storage_db;Username=postgres;Password=123;Include Error Detail=true")
         .Options;

            var db = new SmartStorageContext(options);
            db.Users.Add(new User
            {
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                Name = "Test User",
                Role = "operator"
            });
            db.SaveChanges();
            return db;
        }

        private IConfiguration GetConfig()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"Jwt:Key", "super_secret_test_jwt_key_1234567890_abcdef"}
            };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsToken()
        {
            var db = GetDb();
            var config = GetConfig();
            var controller = new AuthController(db, config);

            var result = controller.Login(new LoginRequest
            {
                Email = "test@example.com",
                Password = "123"
            }) as OkObjectResult;

            Assert.NotNull(result);
            var response = result.Value as LoginResponse;
            Assert.NotNull(response);
            Assert.False(string.IsNullOrEmpty(response.Token));
        }

        [Fact]
        public void Login_InvalidPassword_ReturnsUnauthorized()
        {
            var db = GetDb();
            var config = GetConfig();
            var controller = new AuthController(db, config);

            var result = controller.Login(new LoginRequest
            {
                Email = "test@example.com",
                Password = "wrongpass"
            }) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }
    }
}
