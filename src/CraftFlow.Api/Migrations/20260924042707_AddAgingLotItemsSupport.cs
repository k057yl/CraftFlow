using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAgingLotItemsSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentQuantity",
                schema: "aging",
                table: "aging_lots");

            migrationBuilder.DropColumn(
                name: "UnitsCount",
                schema: "aging",
                table: "aging_lots");

            migrationBuilder.CreateTable(
                name: "aging_lot_items",
                schema: "aging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgingLotId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InitialWeight = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrentWeight = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    DiscardReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aging_lot_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_aging_lot_items_aging_lots_AgingLotId",
                        column: x => x.AgingLotId,
                        principalSchema: "aging",
                        principalTable: "aging_lots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aging_lot_items_AgingLotId",
                schema: "aging",
                table: "aging_lot_items",
                column: "AgingLotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aging_lot_items",
                schema: "aging");

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentQuantity",
                schema: "aging",
                table: "aging_lots",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UnitsCount",
                schema: "aging",
                table: "aging_lots",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
