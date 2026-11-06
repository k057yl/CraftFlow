using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixSubscriptionConfigurationsAndTenantSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tenant_subscriptions_TenantId",
                schema: "saas",
                table: "tenant_subscriptions");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_subscriptions_TenantId",
                schema: "saas",
                table: "tenant_subscriptions",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tenant_subscriptions_TenantId",
                schema: "saas",
                table: "tenant_subscriptions");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_subscriptions_TenantId",
                schema: "saas",
                table: "tenant_subscriptions",
                column: "TenantId",
                unique: true);
        }
    }
}
