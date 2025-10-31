using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartStorageBackend;
using SmartStorageBackend.Hubs;
using System.Text;

// Создайте/отредактируйте appsettings.json: ConnectionStrings:DefaultConnection и (опционально) Jwt:Key

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// JWT key (dev). Лучше переопределять в appsettings.json или через переменные окружения.
var jwtKey = configuration["Jwt:Key"] ?? "ChangeThisSecretInProduction_ReplaceMe!";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

// ----------------- Services -----------------

// EF Core (Postgres)
builder.Services.AddDbContext<SmartStorageContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

// Authentication - JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // в prod = true
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });


// Controllers / Swagger / SignalR / HttpClient
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartStorage API",
        Version = "v1",
        Description = "Backend для системы SmartStorage (роботы, склад, ИИ)",
        Contact = new OpenApiContact
        {
            Name = "Backend Team",
            Email = "ivan20140767@gmail.com"
        }
    });

    // Swagger Bearer support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Введите JWT токен: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

builder.Services.AddSignalR();
builder.Services.AddHttpClient();

var app = builder.Build();

// ----------------- Swagger UI -----------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartStorage API v1");
        c.RoutePrefix = string.Empty; // swagger на корне
    });
}

app.UseHttpsRedirection();
app.UseRouting();

// Обязательно: Authentication перед Authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<DashboardHub>("/api/ws/dashboard");
});

// ----------------- Dev seed: создаём N роботов и пользователей-роботов -----------------
// Настройка: сколько роботов создать в dev (можно брать из конфигурации)
int robotsToSeed = int.TryParse(configuration["Dev:RobotsCount"], out var tmp) ? tmp : 5;

try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SmartStorageContext>();

        // Убедиться, что база существует / применены миграции (в прод — выполнять миграции отдельно)
        // db.Database.EnsureCreated(); // опционально, если не используешь миграции

        // Создадим указанное количество роботов, если ещё нет
        for (int i = 1; i <= robotsToSeed; i++)
        {
            var robotId = $"RB-{i:000}";
            var robotEmail = $"{robotId.ToLower()}@robots.local"; // e.g. rb-001@robots.local

            // Если пользователь с таким email не существует — создаём
            if (!db.Users.Any(u => u.Email == robotEmail))
            {
                var hashed = BCrypt.Net.BCrypt.HashPassword("robotpassword123");
                var user = new SmartStorageBackend.Models.User
                {
                    Email = robotEmail,
                    PasswordHash = hashed,
                    Name = robotId, // используем имя как идентификатор робота
                    Role = "robot",
                    CreatedAt = DateTime.UtcNow
                };
                db.Users.Add(user);
                db.SaveChanges();
            }

            // Если робот с таким Id не существует — создаём
            if (!db.Robots.Any(r => r.Id == robotId))
            {
                var robot = new SmartStorageBackend.Models.Robot
                {
                    Id = robotId,
                    Status = "idle",
                    BatteryLevel = 100,
                    CurrentZone = "A",
                    CurrentRow = 1,
                    CurrentShelf = 1,
                    LastUpdate = DateTime.UtcNow
                };
                db.Robots.Add(robot);
                db.SaveChanges();
            }
        }

        // (Опционально) Создаём admin для тестирования web UI
        if (!db.Users.Any(u => u.Email == "admin@local"))
        {
            var hashedAdmin = BCrypt.Net.BCrypt.HashPassword("adminpassword123");
            db.Users.Add(new SmartStorageBackend.Models.User
            {
                Email = "admin@local",
                PasswordHash = hashedAdmin,
                Name = "Admin",
                Role = "admin",
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }
    }
}
catch (Exception ex)
{
    // Логируем, но не падаем
    Console.WriteLine($"Seed error: {ex.Message}");
}

// ----------------- Run -----------------
app.Run();
