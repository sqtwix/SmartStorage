using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartStorageBackend;
using SmartStorageBackend.Hubs;
using SmartStorageBackend.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------------------- Configuration ----------------------
var configuration = builder.Configuration;

// JWT key (should be in appsettings.json or env variable in real deployment)
var jwtKey = configuration["Jwt:Key"] ?? "ChangeThisSecretInProduction_ReplaceMe!";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

// ---------------------- Services ----------------------
// EF Core (Postgres)
builder.Services.AddDbContext<SmartStorageContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

// Authentication: JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // в dev удобно, в prod включить true
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

// Authorization
builder.Services.AddAuthorization();

// Controllers + Swagger/OpenAPI
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

    // Поддержка авторизации (JWT) в Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Введите JWT токен (пример: Bearer eyJhbGci...)",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// SignalR
builder.Services.AddSignalR();

// HttpClient factory (для интеграций с AI/другими сервисами)
builder.Services.AddHttpClient();

var app = builder.Build();

// ---------------------- Dev: Swagger UI ----------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartStorage API v1");
        c.RoutePrefix = string.Empty; // Swagger UI доступен на корне
    });
}

// ---------------------- Middleware pipeline ----------------------
app.UseHttpsRedirection();

app.UseRouting();

// Authentication & Authorization MUST be between Routing and Endpoints
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints (controllers + SignalR)
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<DashboardHub>("/api/ws/dashboard");
});

// ---------------------- Seed dev robot user (synchronous, safe for small seed) ----------------------
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<SmartStorageContext>();

        // Ensure database is created (if using EnsureCreated approach). If you use migrations prefer Update-Database/EF commands.
        // db.Database.EnsureCreated();


        // If Users set exists, create a dev robot user if not present.
        // NOTE: adjust to your actual Users DbSet and User model properties.
        var hasUsers = db.Users.Any();
        // If there are no users at all, create an example robot account (dev only).
        if (!db.Users.Any(u => u.Email == "robot1@local"))
        {
            var hashed = BCrypt.Net.BCrypt.HashPassword("robotpassword123");
            db.Users.Add(new SmartStorageBackend.Models.User
            {
                Email = "robot1@local",
                PasswordHash = hashed,
                Name = "Robot 1",
                Role = "robot",
                CreatedAt = DateTime.UtcNow
            });
            db.SaveChanges();
        }

        // Optionally, create an admin account for frontend testing if none exists
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
    // В dev логируем, чтобы не падал запуск. В prod — обработать корректно.
    Console.WriteLine($"Seed error: {ex.Message}");
}

// ---------------------- Run ----------------------
app.Run();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SmartStorageContext>();

    // Проверяем, есть ли уже пользователи-роботы
    if (!db.Users.Any(u => u.Role == "robot"))
    {
        for (int i = 1; i <= 5; i++) // например, 5 роботов
        {
            var email = $"robot{i}@local";
            var user = new User
            {
                Email = email,
                Name = $"Robot {i}",
                Role = "robot",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("robotpassword123"),
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            db.SaveChanges(); // чтобы получить Id пользователя

            var robot = new Robot
            {
                Id = $"RB-{i:000}", // RB-001, RB-002 и т.п.
                Status = "idle",
                BatteryLevel = 100,
                CurrentZone = "A",
                CurrentRow = 1,
                CurrentShelf = 1,
                LastUpdate = DateTime.UtcNow,
                UserId = user.Id // связь с пользователем
            };

            db.Robots.Add(robot);
        }

        db.SaveChanges();
    }
}