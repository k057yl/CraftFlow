using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSaasSchemaAndFinalIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "stock_lot_storage_locations",
                schema: "public",
                newName: "stock_lot_storage_locations",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "audit_logs",
                schema: "public",
                newName: "audit_logs",
                newSchema: "identity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "stock_lot_storage_locations",
                schema: "inventory",
                newName: "stock_lot_storage_locations",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "audit_logs",
                schema: "identity",
                newName: "audit_logs",
                newSchema: "public");
        }
    }
}
