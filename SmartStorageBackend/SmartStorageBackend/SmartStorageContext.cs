using Microsoft.EntityFrameworkCore;
using SmartStorageBackend.Models;

namespace SmartStorageBackend
{
    public class SmartStorageContext : DbContext
    {
        public SmartStorageContext(DbContextOptions<SmartStorageContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Robot> Robots { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<InventoryHistory> InventoryHistory { get; set; }
        public DbSet<AiPrediction> AiPredictions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Уникальный Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Индексы для InventoryHistory
            modelBuilder.Entity<InventoryHistory>()
                .HasIndex(i => i.ScannedAt)
                .HasDatabaseName("idx_inventory_scanned");

            modelBuilder.Entity<InventoryHistory>()
                .HasIndex(i => i.ProductId)
                .HasDatabaseName("idx_inventory_product");

            modelBuilder.Entity<InventoryHistory>()
                .HasIndex(i => i.Zone)
                .HasDatabaseName("idx_inventory_zone");

            // Связи Robot → InventoryHistory
            modelBuilder.Entity<InventoryHistory>()
                .HasOne(i => i.Robot)
                .WithMany()
                .HasForeignKey(i => i.RobotId)
                .OnDelete(DeleteBehavior.SetNull);

            // Связи Product → InventoryHistory, Product → AiPrediction
            modelBuilder.Entity<InventoryHistory>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AiPrediction>()
                .HasOne(a => a.Product)
                .WithMany()
                .HasForeignKey(a => a.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
