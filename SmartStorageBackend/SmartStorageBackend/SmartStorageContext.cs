using Microsoft.EntityFrameworkCore;
using SmartStorageBackend.Models;
using Microsoft.EntityFrameworkCore.Design;


namespace SmartStorageBackend
{
    public class SmartStorageContext : DbContext
    {
        public SmartStorageContext(DbContextOptions<SmartStorageContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
       
    }
}
