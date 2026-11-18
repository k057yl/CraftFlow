using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class DecoupleFermentationFromProduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FERMENTATION_BATCHES_PRODUCTION_BATCH",
                schema: "fermentation",
                table: "fermentation_batches");

            migrationBuilder.RenameColumn(
                name: "ProductionBatchId",
                schema: "fermentation",
                table: "fermentation_batches",
                newName: "WarehouseId");

            migrationBuilder.AddColumn<decimal>(
                name: "ActualOutputQuantity",
                schema: "fermentation",
                table: "fermentation_batches",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "DestinationWarehouseId",
                schema: "fermentation",
                table: "fermentation_batches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "DiscardReason",
                schema: "fermentation",
                table: "fermentation_batches",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscardedAt",
                schema: "fermentation",
                table: "fermentation_batches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EquipmentLocationId",
                schema: "fermentation",
                table: "fermentation_batches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "OverheadPercentage",
                schema: "fermentation",
                table: "fermentation_batches",
                type: "numeric(6,2)",
                precision: 6,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecipeId",
                schema: "fermentation",
                table: "fermentation_batches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TargetProductId",
                schema: "fermentation",
                table: "fermentation_batches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "UnitsCount",
                schema: "fermentation",
                table: "fermentation_batches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "fermentation_consumed_ingredients",
                schema: "fermentation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FermentationBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockLotId = table.Column<Guid>(type: "uuid", nullable: false),
                    RawMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fermentation_consumed_ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fermentation_consumed_ingredients_fermentation_batches_Ferm~",
                        column: x => x.FermentationBatchId,
                        principalSchema: "fermentation",
                        principalTable: "fermentation_batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FERMENTATION_CONSUMED_INGREDIENTS_BATCH_ID",
                schema: "fermentation",
                table: "fermentation_consumed_ingredients",
                column: "FermentationBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FERMENTATION_CONSUMED_INGREDIENTS_TENANT_ID",
                schema: "fermentation",
                table: "fermentation_consumed_ingredients",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fermentation_consumed_ingredients",
                schema: "fermentation");

            migrationBuilder.DropColumn(
                name: "ActualOutputQuantity",
                schema: "fermentation",
                table: "fermentation_batches");

            migrationBuilder.DropColumn(
                name: "DestinationWarehouseId",
                schema: "fermentation",
                table: "fermentation_batches");

            migrationBuilder.DropColumn(
                name: "DiscardReason",
                schema: "fermentation",
                table: "fermentation_batches");

            migrationBuilder.DropColumn(
                name: "DiscardedAt",
                schema: "fermentation",
                table: "fermentation_batches");

            migrationBuilder.DropColumn(
                name: "EquipmentLocationId",
                schema: "fermentation",
                table: "fermentation_batches");

            migrationBuilder.DropColumn(
                name: "OverheadPercentage",
                schema: "fermentation",
                table: "fermentation_batches");

            migrationBuilder.DropColumn(
                name: "RecipeId",
                schema: "fermentation",
                table: "fermentation_batches");

            migrationBuilder.DropColumn(
                name: "TargetProductId",
                schema: "fermentation",
                table: "fermentation_batches");

            migrationBuilder.DropColumn(
                name: "UnitsCount",
                schema: "fermentation",
                table: "fermentation_batches");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                schema: "fermentation",
                table: "fermentation_batches",
                newName: "ProductionBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FERMENTATION_BATCHES_PRODUCTION_BATCH",
                schema: "fermentation",
                table: "fermentation_batches",
                column: "ProductionBatchId");
        }
    }
}
