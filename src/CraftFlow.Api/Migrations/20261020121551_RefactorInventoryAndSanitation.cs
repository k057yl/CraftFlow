using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class RefactorInventoryAndSanitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentVolume",
                schema: "inventory",
                table: "storage_locations",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.CreateIndex(
                name: "IX_STORAGE_LOCATIONS_TENANT_WAREHOUSE",
                schema: "inventory",
                table: "storage_locations",
                columns: new[] { "TenantId", "WarehouseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_STORAGE_LOCATIONS_TENANT_WAREHOUSE",
                schema: "inventory",
                table: "storage_locations");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentVolume",
                schema: "inventory",
                table: "storage_locations",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);
        }
    }
}
