using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class Addstocklotfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductId",
                schema: "public",
                table: "sales_order_items",
                newName: "StockLotId");

            migrationBuilder.AddColumn<int>(
                name: "UnitsCount",
                schema: "public",
                table: "stock_lots",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "public",
                table: "sales_orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedAt",
                schema: "public",
                table: "sales_orders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitsCount",
                schema: "public",
                table: "stock_lots");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "public",
                table: "sales_orders");

            migrationBuilder.DropColumn(
                name: "ShippedAt",
                schema: "public",
                table: "sales_orders");

            migrationBuilder.RenameColumn(
                name: "StockLotId",
                schema: "public",
                table: "sales_order_items",
                newName: "ProductId");
        }
    }
}
