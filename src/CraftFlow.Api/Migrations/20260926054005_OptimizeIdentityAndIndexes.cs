using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeIdentityAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_users_Email",
                schema: "public",
                table: "users",
                newName: "IX_USERS_EMAIL");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_TENANT_ROLE_ACTIVE",
                schema: "public",
                table: "users",
                columns: new[] { "TenantId", "Role", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTION_BATCHES_TIMER_MONITORING",
                schema: "public",
                table: "production_batches",
                columns: new[] { "State", "IsTelegramNotified", "StartedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_USERS_TENANT_ROLE_ACTIVE",
                schema: "public",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_PRODUCTION_BATCHES_TIMER_MONITORING",
                schema: "public",
                table: "production_batches");

            migrationBuilder.RenameIndex(
                name: "IX_USERS_EMAIL",
                schema: "public",
                table: "users",
                newName: "IX_users_Email");
        }
    }
}
