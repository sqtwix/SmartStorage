using Microsoft.EntityFrameworkCore;
using SmartStorageBackend;
using SmartStorageBackend.Hubs;
using Microsoft.OpenApi.Models;



var builder = WebApplication.CreateBuilder(args);

// ���������� EF Core
builder.Services.AddDbContext<SmartStorageContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ��������� ����������� � Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartStorage API",
        Version = "v1",
        Description = "Backend ��� ������� SmartStorage (������, �����, ��)",
        Contact = new OpenApiContact
        {
            Name = "Backend Team",
            Email = "ivan20140767@gmail.com.com"
        }
    });

    // ��������� ����������� (JWT)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "������� JWT ����� (������: Bearer eyJhbGci...)",
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

// ��������� SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Swagger UI � OpenAPI JSON
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
