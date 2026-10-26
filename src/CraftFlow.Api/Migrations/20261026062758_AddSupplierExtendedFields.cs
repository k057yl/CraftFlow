using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "procurement",
                table: "suppliers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalName",
                schema: "procurement",
                table: "suppliers",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "procurement",
                table: "suppliers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxId",
                schema: "procurement",
                table: "suppliers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SUPPLIERS_TENANT_TAX_ID",
                schema: "procurement",
                table: "suppliers",
                columns: new[] { "TenantId", "TaxId" },
                filter: "\"TaxId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SUPPLIERS_TENANT_TAX_ID",
                schema: "procurement",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "Address",
                schema: "procurement",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "LegalName",
                schema: "procurement",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "procurement",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "TaxId",
                schema: "procurement",
                table: "suppliers");
        }
    }
}
