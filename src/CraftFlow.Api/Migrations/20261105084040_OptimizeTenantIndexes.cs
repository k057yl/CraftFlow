using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeTenantIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SALES_ORDERS_STATUS",
                schema: "sales",
                table: "sales_orders");

            migrationBuilder.DropIndex(
                name: "IX_PURCHASE_ORDERS_STATUS",
                schema: "procurement",
                table: "purchase_orders");

            migrationBuilder.DropIndex(
                name: "IX_PRODUCTION_BATCHES_TIMER_MONITORING",
                schema: "production",
                table: "production_batches");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_ORDERS_TENANT_STATUS",
                schema: "sales",
                table: "sales_orders",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PURCHASE_ORDERS_TENANT_STATUS",
                schema: "procurement",
                table: "purchase_orders",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTION_BATCHES_TIMER_MONITORING",
                schema: "production",
                table: "production_batches",
                columns: new[] { "TenantId", "State", "IsTelegramNotified", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AUDIT_LOGS_TENANT_CREATED_AT",
                schema: "identity",
                table: "audit_logs",
                columns: new[] { "TenantId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AGING_LOTS_TENANT_STATE",
                schema: "aging",
                table: "aging_lots",
                columns: new[] { "TenantId", "State" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SALES_ORDERS_TENANT_STATUS",
                schema: "sales",
                table: "sales_orders");

            migrationBuilder.DropIndex(
                name: "IX_PURCHASE_ORDERS_TENANT_STATUS",
                schema: "procurement",
                table: "purchase_orders");

            migrationBuilder.DropIndex(
                name: "IX_PRODUCTION_BATCHES_TIMER_MONITORING",
                schema: "production",
                table: "production_batches");

            migrationBuilder.DropIndex(
                name: "IX_AUDIT_LOGS_TENANT_CREATED_AT",
                schema: "identity",
                table: "audit_logs");

            migrationBuilder.DropIndex(
                name: "IX_AGING_LOTS_TENANT_STATE",
                schema: "aging",
                table: "aging_lots");

            migrationBuilder.CreateIndex(
                name: "IX_SALES_ORDERS_STATUS",
                schema: "sales",
                table: "sales_orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PURCHASE_ORDERS_STATUS",
                schema: "procurement",
                table: "purchase_orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTION_BATCHES_TIMER_MONITORING",
                schema: "production",
                table: "production_batches",
                columns: new[] { "State", "IsTelegramNotified", "StartedAt" });
        }
    }
}
