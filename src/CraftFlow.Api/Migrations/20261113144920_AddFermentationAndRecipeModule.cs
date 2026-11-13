using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFermentationAndRecipeModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fermentation");

            migrationBuilder.AddColumn<string>(
                name: "TargetModule",
                schema: "catalog",
                table: "recipes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "CHEESE_MAKING");

            migrationBuilder.CreateTable(
                name: "fermentation_batches",
                schema: "fermentation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductionBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecipeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SubstrateWeightKg = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    SaltPercentage = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    InitialPh = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    CurrentPh = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    TargetTemperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    VesselInfo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Stage = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MovedToColdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fermentation_batches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fermentation_readings",
                schema: "fermentation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FermentationBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ph = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    Temperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    DegassedOrBurped = table.Column<bool>(type: "boolean", nullable: false),
                    BrineToppedUp = table.Column<bool>(type: "boolean", nullable: false),
                    SensoryNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fermentation_readings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fermentation_readings_fermentation_batches_FermentationBatc~",
                        column: x => x.FermentationBatchId,
                        principalSchema: "fermentation",
                        principalTable: "fermentation_batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FERMENTATION_BATCHES_PRODUCTION_BATCH",
                schema: "fermentation",
                table: "fermentation_batches",
                column: "ProductionBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FERMENTATION_BATCHES_TENANT_STAGE",
                schema: "fermentation",
                table: "fermentation_batches",
                columns: new[] { "TenantId", "Stage" });

            migrationBuilder.CreateIndex(
                name: "IX_FERMENTATION_READINGS_BATCH_RECORDED_AT",
                schema: "fermentation",
                table: "fermentation_readings",
                columns: new[] { "FermentationBatchId", "RecordedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fermentation_readings",
                schema: "fermentation");

            migrationBuilder.DropTable(
                name: "fermentation_batches",
                schema: "fermentation");

            migrationBuilder.DropColumn(
                name: "TargetModule",
                schema: "catalog",
                table: "recipes");
        }
    }
}
