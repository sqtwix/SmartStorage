using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using SmartStorageBackend;
using SmartStorageBackend.Controllers;
using SmartStorageBackend.DTOs;
using SmartStorageBackend.Models;

public class AuthControllerTests : IDisposable
{
    private readonly SmartStorageContext _db;
    private readonly IDbContextTransaction _transaction;

    public AuthControllerTests()
    {
        var options = new DbContextOptionsBuilder<SmartStorageContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=smart_storage_db;Username=postgres;Password=123;Include Error Detail=true")
            .Options;

        _db = new SmartStorageContext(options);
        _transaction = _db.Database.BeginTransaction();

        // Создаем тестового пользователя один раз
        if (!_db.Users.Any(u => u.Email == "test@example.com"))
        {
            _db.Users.Add(new User
            {
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                Name = "Test User",
                Role = "operator"
            });
            _db.SaveChanges();
        }
    }

    public void Dispose()
    {
        _transaction?.Rollback();
        _db?.Dispose();
    }

    private IConfiguration GetConfig()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Key", "SmartStorageSuperSecretKey_2025_AI_Robotics_1234567890"}
        };
        return new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public void Login_ValidCredentials_ReturnsToken()
    {
        var config = GetConfig();
        var controller = new AuthController(_db, config);

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
        var config = GetConfig();
        var controller = new AuthController(_db, config);

        var result = controller.Login(new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpass"
        }) as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(401, result.StatusCode);
    }
}