using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartStorageBackend.Migrations
{
    /// <inheritdoc />
    public partial class PredictModelAndInventoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiPredictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PredictionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DaysUntilStockout = table.Column<int>(type: "integer", nullable: true),
                    RecommendedOrder = table.Column<int>(type: "integer", nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(3,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiPredictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiPredictions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RobotId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProductId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Zone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: true),
                    ShelfNumber = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScannedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryHistory_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryHistory_Robots_RobotId",
                        column: x => x.RobotId,
                        principalTable: "Robots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AiPredictions_ProductId",
                table: "AiPredictions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "idx_inventory_product",
                table: "InventoryHistory",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "idx_inventory_scanned",
                table: "InventoryHistory",
                column: "ScannedAt");

            migrationBuilder.CreateIndex(
                name: "idx_inventory_zone",
                table: "InventoryHistory",
                column: "Zone");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryHistory_RobotId",
                table: "InventoryHistory",
                column: "RobotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiPredictions");

            migrationBuilder.DropTable(
                name: "InventoryHistory");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");
        }
    }
}
