using CsvHelper.Configuration;

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

    public sealed class InventoryCsvRowMap : ClassMap<InventoryCsvRow>
    {
        public InventoryCsvRowMap()
        {
            Map(m => m.ProductId).Name("product_id");
            Map(m => m.ProductName).Name("product_name");
            Map(m => m.Quantity).Name("quantity");
            Map(m => m.Zone).Name("zone");
            Map(m => m.Date).Name("date");
            Map(m => m.Row).Name("row");
            Map(m => m.Shelf).Name("shelf");
        }
    }
}
