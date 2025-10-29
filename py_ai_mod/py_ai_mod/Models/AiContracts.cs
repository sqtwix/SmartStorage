using System;
using System.Collections.Generic;

namespace py_ai_mod.Models
{
    // Contracts matching SmartStorageBackend expectations for /predict
    public record AiPredictRequest
    {
        public int PeriodDays { get; init; } = 7;
        public List<string>? Categories { get; init; }
    }

    public record AiPredictResponse
    {
        public List<AiPredictionDTO> Predictions { get; init; } = new();
        public decimal Confidence { get; init; }
    }

    public record AiPredictionDTO
    {
        public string ProductId { get; init; } = string.Empty;
        public DateTime PredictionDate { get; init; } = DateTime.UtcNow;
        public int? DaysUntilStockout { get; init; }
        public int? RecommendedOrder { get; init; }
        public decimal? ConfidenceScore { get; init; }
    }
}


