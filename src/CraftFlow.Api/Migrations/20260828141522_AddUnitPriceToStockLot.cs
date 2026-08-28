using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitPriceToStockLot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "stock_lots",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "stock_lots");
        }
    }
}
