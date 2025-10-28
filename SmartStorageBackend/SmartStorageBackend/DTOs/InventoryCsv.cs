namespace SmartStorageBackend.DTOs
{
    public class InventoryCsvRow
    {
        public string ProductId { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public string Zone { get; set; } = null!;
        public DateTime Date { get; set; }
        public int Row { get; set; }
        public int Shelf { get; set; }
    }
}