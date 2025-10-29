using Microsoft.EntityFrameworkCore;
using SmartStorageBackend;
using SmartStorageBackend.Hubs;
using Microsoft.OpenApi.Models;



var builder = WebApplication.CreateBuilder(args);

// Подключаем EF Core
builder.Services.AddDbContext<SmartStorageContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем контроллеры и Swagger
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
            Email = "ivan20140767@gmail.com.com"
        }
    });

    // Поддержка авторизации (JWT)
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

// Добавляем SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Swagger UI и OpenAPI JSON
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartStorage API v1");
        c.RoutePrefix = ""; 
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<DashboardHub>("/api/ws/dashboard");
});

app.Run();
