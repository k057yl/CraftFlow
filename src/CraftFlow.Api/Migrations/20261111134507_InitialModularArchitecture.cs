using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialModularArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cheese_making");

            migrationBuilder.RenameTable(
                name: "aging_lots",
                schema: "aging",
                newName: "aging_lots",
                newSchema: "cheese_making");

            migrationBuilder.RenameTable(
                name: "aging_lot_items",
                schema: "aging",
                newName: "aging_lot_items",
                newSchema: "cheese_making");

            migrationBuilder.RenameTable(
                name: "aging_chambers",
                schema: "aging",
                newName: "aging_chambers",
                newSchema: "cheese_making");

            migrationBuilder.CreateTable(
                name: "subscription_invoices",
                schema: "saas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DaysRequested = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_invoices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_subscription_invoices_Status",
                schema: "saas",
                table: "subscription_invoices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_subscription_invoices_TenantId",
                schema: "saas",
                table: "subscription_invoices",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "subscription_invoices",
                schema: "saas");

            migrationBuilder.EnsureSchema(
                name: "aging");

            migrationBuilder.RenameTable(
                name: "aging_lots",
                schema: "cheese_making",
                newName: "aging_lots",
                newSchema: "aging");

            migrationBuilder.RenameTable(
                name: "aging_lot_items",
                schema: "cheese_making",
                newName: "aging_lot_items",
                newSchema: "aging");

            migrationBuilder.RenameTable(
                name: "aging_chambers",
                schema: "cheese_making",
                newName: "aging_chambers",
                newSchema: "aging");
        }
    }
}
