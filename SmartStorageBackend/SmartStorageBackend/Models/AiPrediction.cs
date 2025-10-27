using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStorageBackend.Models
{
    public class AiPrediction
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Product))]
        [MaxLength(50)]
        public string ProductId { get; set; } = null!;
        public Product Product { get; set; } = null!;

        [Required]
        public DateTime PredictionDate { get; set; } = DateTime.UtcNow;

        public int? DaysUntilStockout { get; set; }
        public int? RecommendedOrder { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal? ConfidenceScore { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
