using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class Addfieldfromstorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Organizations",
                schema: "public",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "StorageLocationId",
                schema: "public",
                table: "stock_lots");

            migrationBuilder.RenameTable(
                name: "Organizations",
                schema: "public",
                newName: "organizations",
                newSchema: "public");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                schema: "public",
                table: "stock_lots",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "BatchNumber",
                schema: "public",
                table: "stock_lots",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_organizations",
                schema: "public",
                table: "organizations",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "stock_lot_storage_locations",
                schema: "public",
                columns: table => new
                {
                    StockLotId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocatedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_lot_storage_locations", x => new { x.StockLotId, x.StorageLocationId });
                    table.ForeignKey(
                        name: "FK_stock_lot_storage_locations_stock_lots_StockLotId",
                        column: x => x.StockLotId,
                        principalSchema: "public",
                        principalTable: "stock_lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_lot_storage_locations",
                schema: "public");

            migrationBuilder.DropPrimaryKey(
                name: "PK_organizations",
                schema: "public",
                table: "organizations");

            migrationBuilder.RenameTable(
                name: "organizations",
                schema: "public",
                newName: "Organizations",
                newSchema: "public");

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                schema: "public",
                table: "stock_lots",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<string>(
                name: "BatchNumber",
                schema: "public",
                table: "stock_lots",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StorageLocationId",
                schema: "public",
                table: "stock_lots",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Organizations",
                schema: "public",
                table: "Organizations",
                column: "Id");
        }
    }
}
