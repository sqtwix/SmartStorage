using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartStorageBackend;
using SmartStorageBackend.Hubs;
using System.Text;

// Настройка/перенастройка appsettings.json: ConnectionStrings:DefaultConnection и (опционально) Jwt:Key

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// JWT key (dev). Можно перенастроить в appsettings.json или использовать переменное окружения.
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
    
    // Настройка для SignalR: получение токена из Query String или Header
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var path = context.HttpContext.Request.Path;
            
            // Для WebSocket соединений проверяем query string и заголовок
            if (path.StartsWithSegments("/api/ws"))
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                // Если токена нет в query string, проверяем заголовок Authorization
                else
                {
                    var authHeader = context.Request.Headers["Authorization"].ToString();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    {
                        context.Token = authHeader.Substring("Bearer ".Length).Trim();
                    }
                }
            }

            return Task.CompletedTask;
        }
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
        Description = "Вставьте JWT токен: Bearer {token}",
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

// CORS для фронтенда
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5171", "http://localhost")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Важно для SignalR
    });
});

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
else
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors("AllowFrontend");

// Важно: Authentication перед Authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<DashboardHub>("/api/ws/dashboard");
});

// ----------------- Dev seed: создание N роботов и администратора-юзера -----------------
// Замечание: только для dev режима (этого достаточно для тестирования)
int robotsToSeed = int.TryParse(configuration["Dev:RobotsCount"], out var tmp) ? tmp : 5;

try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SmartStorageContext>();

        // Проверяем, что база инициализирована / миграции применены (в продакшене выполняются миграции отдельно)
        // db.Database.EnsureCreated(); // только если миграции не применены

        // Создаём минимальные пользователи для роботов, если их нет
        for (int i = 1; i <= robotsToSeed; i++)
        {
            var robotId = $"RB-{i:000}";
            var robotEmail = $"{robotId.ToLower()}@robots.local"; // e.g. rb-001@robots.local

            // если пользователь с таким email не находится в базе
            if (!db.Users.Any(u => u.Email == robotEmail))
            {
                var hashed = BCrypt.Net.BCrypt.HashPassword("robotpassword123");
                var user = new SmartStorageBackend.Models.User
                {
                    Email = robotEmail,
                    PasswordHash = hashed,
                    Name = robotId, // отображаемое имя как идентификатор робота
                    Role = "robot",
                    CreatedAt = DateTime.UtcNow
                };
                db.Users.Add(user);
                db.SaveChanges();
            }

            // если робот с таким Id не находится в базе
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

        // (опционально) создаём admin для администрирования web UI
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
    // игнорируем, по возможности логируем
    Console.WriteLine($"Seed error: {ex.Message}");
}

// ----------------- Run -----------------
app.Run();