namespace SmartStorageBackend.DTOs
{
    public class InventoryHistoryDTO
    {
        public string? RobotId { get; set; }
        public string? ProductId { get; set; }
        public int Quantity { get; set; }
        public string Zone { get; set; } = null!;
        public int? RowNumber { get; set; }
        public int? ShelfNumber { get; set; }
        public string Status { get; set; } = "OK";
        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
    }
}
