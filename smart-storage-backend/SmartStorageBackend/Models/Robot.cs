using System.ComponentModel.DataAnnotations;

namespace SmartStorageBackend.Models
{
    public class Robot
    {  
            [Key]
            [MaxLength(50)]
            public string Id { get; set; } = null!;

            [MaxLength(50)]
            public string Status { get; set; } = "active";

            public int BatteryLevel { get; set; }

            [MaxLength(50)]
            public string? CurrentZone { get; set; }
            public int? CurrentRow { get; set; }
            public int? CurrentShelf { get; set; }

            public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
        
    }
}
