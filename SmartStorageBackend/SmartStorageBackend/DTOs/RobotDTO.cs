namespace SmartStorageBackend.DTOs
{
    public class RobotDataDTO
    {
        public string RobotId { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public RobotLocationDTO Location { get; set; } = null!;
        public List<ScanResultDTO> ScanResults { get; set; } = new();

        public int BatteryLevel { get; set; }
    }

    public class RobotLocationDTO
    {
        public string Zone { get; set; } = null!;
        public int Row { get; set; }
        public int Shelf { get; set; }
    }

    public class ScanResultDTO
    {
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }
        public string Status { get; set; } = "OK"; // 'OK', 'LOW_STOCK', 'CRITICAL'
    }
}

