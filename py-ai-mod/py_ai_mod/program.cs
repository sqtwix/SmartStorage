using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using py_ai_mod.Services;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.Generic;
using py_ai_mod.Models;

var builder = WebApplication.CreateBuilder(args);

// Регистрация сервисов
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Добавляем твой сервис работы с AI
builder.Services.AddHttpClient<IPyAiService, PyAiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// Lightweight compatibility endpoint for SmartStorageBackend AiController
// Exposes POST /predict expecting AiPredictRequest and returning AiPredictResponse
app.MapPost("/predict", (AiPredictRequest request) =>
{
    // Return a minimal valid response; Python AI can be integrated later inside IPyAiService
    return Results.Ok(new AiPredictResponse
    {
        Predictions = new List<AiPredictionDTO>(),
        Confidence = 0.9m
    });
})
;

// Старт приложения (будет использовать ASPNETCORE_URLS из переменных окружения, например "http://+:8000")
// Это позволяет принимать соединения из других контейнеров Docker
app.Run();
