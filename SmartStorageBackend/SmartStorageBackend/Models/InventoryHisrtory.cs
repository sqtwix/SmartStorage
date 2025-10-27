using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStorageBackend.Models
{
    public class InventoryHistory
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Robot))]
        [MaxLength(50)]
        public string? RobotId { get; set; }
        public Robot? Robot { get; set; }

        [ForeignKey(nameof(Product))]
        [MaxLength(50)]
        public string? ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required, MaxLength(10)]
        public string Zone { get; set; } = null!;

        public int? RowNumber { get; set; }
        public int? ShelfNumber { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "OK"; // OK, LOW_STOCK, CRITICAL

        [Required]
        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
