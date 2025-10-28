namespace SmartStorageBackend.DTOs
{
    public class InventoryCsvRow
    {
        public string ProductId { get; set; } = null!;
        public string? RobotId { get; set; }
        public string Zone { get; set; } = null!;
        public int RowNumber { get; set; }
        public int ShelfNumber { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = "OK";
    }
}