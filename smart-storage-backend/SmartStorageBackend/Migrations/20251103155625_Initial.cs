using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartStorageBackend.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Optimal_stock",
                table: "Products",
                newName: "optimal_stock");

            migrationBuilder.RenameColumn(
                name: "Min_stock",
                table: "Products",
                newName: "min_stock");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "optimal_stock",
                table: "Products",
                newName: "Optimal_stock");

            migrationBuilder.RenameColumn(
                name: "min_stock",
                table: "Products",
                newName: "Min_stock");
        }
    }
}
