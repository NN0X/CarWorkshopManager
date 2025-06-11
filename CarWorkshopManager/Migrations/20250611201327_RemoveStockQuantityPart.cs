using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarWorkshopManager.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStockQuantityPart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "Parts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "Parts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
