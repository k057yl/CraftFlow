using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial_ModularSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddonModules",
                schema: "saas",
                table: "tenant_subscriptions");

            migrationBuilder.RenameColumn(
                name: "BaseModules",
                schema: "saas",
                table: "subscription_plans",
                newName: "TargetModule");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TargetModule",
                schema: "saas",
                table: "subscription_plans",
                newName: "BaseModules");

            migrationBuilder.AddColumn<int>(
                name: "AddonModules",
                schema: "saas",
                table: "tenant_subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
