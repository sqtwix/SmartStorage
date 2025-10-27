namespace SmartStorageBackend.DTOs
{
    public class AiPredictionDTO
    {
        public string ProductId { get; set; } = null!;
        public DateTime PredictionDate { get; set; } = DateTime.UtcNow;
        public int? DaysUntilStockout { get; set; }
        public int? RecommendedOrder { get; set; }
        public decimal? ConfidenceScore { get; set; }
    }

    // DTO для запроса от клиента
    public class AiPredictRequest
    {
        public int PeriodDays { get; set; } = 7;
        public List<string>? Categories { get; set; }
    }

    // DTO для ответа клиенту
    public class AiPredictResponse
    {
        public List<AiPredictionDTO> Predictions { get; set; } = new();
        public decimal Confidence { get; set; }
    }
}
