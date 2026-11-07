using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EnabledModules",
                schema: "saas",
                table: "tenant_subscriptions",
                newName: "AddonModules");

            migrationBuilder.AddColumn<int>(
                name: "BaseModules",
                schema: "saas",
                table: "subscription_plans",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseModules",
                schema: "saas",
                table: "subscription_plans");

            migrationBuilder.RenameColumn(
                name: "AddonModules",
                schema: "saas",
                table: "tenant_subscriptions",
                newName: "EnabledModules");
        }
    }
}
