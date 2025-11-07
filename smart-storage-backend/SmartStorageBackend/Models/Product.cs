using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStorageBackend.Models
{
    public class Product
    {
        [Key]
        [MaxLength(50)]
        public string Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        [Column("min_stock")]
        public int Min_stock { get; set; } = 10;

        [Column("optimal_stock")]
        public int Optimal_stock { get; set; } = 100;

    }
}
