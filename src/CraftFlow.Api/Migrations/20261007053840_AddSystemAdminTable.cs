using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemAdminTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSuperAdmin",
                schema: "identity",
                table: "users");

            migrationBuilder.RenameIndex(
                name: "IX_organization_members_tenant_user",
                schema: "identity",
                table: "organization_members",
                newName: "IX_MEMBERS_TENANT_USER");

            migrationBuilder.RenameIndex(
                name: "IX_organization_members_tenant_role_active",
                schema: "identity",
                table: "organization_members",
                newName: "IX_MEMBERS_TENANT_ROLE_ACTIVE");

            migrationBuilder.CreateTable(
                name: "system_admins",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_admins", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SYSTEM_ADMINS_USER_ID",
                schema: "identity",
                table: "system_admins",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_admins",
                schema: "identity");

            migrationBuilder.RenameIndex(
                name: "IX_MEMBERS_TENANT_USER",
                schema: "identity",
                table: "organization_members",
                newName: "IX_organization_members_tenant_user");

            migrationBuilder.RenameIndex(
                name: "IX_MEMBERS_TENANT_ROLE_ACTIVE",
                schema: "identity",
                table: "organization_members",
                newName: "IX_organization_members_tenant_role_active");

            migrationBuilder.AddColumn<bool>(
                name: "IsSuperAdmin",
                schema: "identity",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
