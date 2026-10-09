using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToEnumStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ORGANIZATIONS_RETENTION_CHECK",
                schema: "identity",
                table: "organizations");

            migrationBuilder.DropIndex(
                name: "IX_MEMBERS_TENANT_ROLE_ACTIVE",
                schema: "identity",
                table: "organization_members");

            migrationBuilder.DropColumn(
                name: "IsSelfDeactivated",
                schema: "identity",
                table: "organizations");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "identity",
                table: "users",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "identity",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "identity",
                table: "organizations",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "identity",
                table: "organizations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "identity",
                table: "organization_members",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "identity",
                table: "organization_members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ORGANIZATIONS_RETENTION_CHECK",
                schema: "identity",
                table: "organizations",
                columns: new[] { "Status", "DeactivatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MEMBERS_TENANT_ROLE_ACTIVE",
                schema: "identity",
                table: "organization_members",
                columns: new[] { "TenantId", "Role", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ORGANIZATIONS_RETENTION_CHECK",
                schema: "identity",
                table: "organizations");

            migrationBuilder.DropIndex(
                name: "IX_MEMBERS_TENANT_ROLE_ACTIVE",
                schema: "identity",
                table: "organization_members");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "identity",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "identity",
                table: "organization_members");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "identity",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "identity",
                table: "organizations",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<bool>(
                name: "IsSelfDeactivated",
                schema: "identity",
                table: "organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "identity",
                table: "organization_members",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.CreateIndex(
                name: "IX_ORGANIZATIONS_RETENTION_CHECK",
                schema: "identity",
                table: "organizations",
                columns: new[] { "IsActive", "IsSelfDeactivated", "DeactivatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MEMBERS_TENANT_ROLE_ACTIVE",
                schema: "identity",
                table: "organization_members",
                columns: new[] { "TenantId", "Role", "IsActive" });
        }
    }
}
