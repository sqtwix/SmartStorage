namespace SmartStorageBackend.DTOs
{
    public class ProductDTO
    {

        public string Name { get; set; }

        public string Category { get; set; }

        public int Min_stock { get; set; } = 10;
        public int Optimal_stock { get; set; } = 100;
    }
}
