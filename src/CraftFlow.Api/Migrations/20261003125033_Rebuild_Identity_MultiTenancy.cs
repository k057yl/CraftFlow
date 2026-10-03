using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class Rebuild_Identity_MultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_USERS_TENANT_ROLE_ACTIVE",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Role",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                schema: "identity",
                table: "users");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "identity",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSuperAdmin",
                schema: "identity",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSelfDeactivated",
                schema: "identity",
                table: "organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.CreateTable(
                name: "organization_members",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    JoinedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_organization_members_organizations_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "identity",
                        principalTable: "organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_members_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_tenant_role_active",
                schema: "identity",
                table: "organization_members",
                columns: new[] { "TenantId", "Role", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_tenant_user",
                schema: "identity",
                table: "organization_members",
                columns: new[] { "TenantId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_members_UserId",
                schema: "identity",
                table: "organization_members",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "organization_members",
                schema: "identity");

            migrationBuilder.DropColumn(
                name: "IsSuperAdmin",
                schema: "identity",
                table: "users");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "identity",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                schema: "identity",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                schema: "identity",
                table: "users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<bool>(
                name: "IsSelfDeactivated",
                schema: "identity",
                table: "organizations",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_TENANT_ROLE_ACTIVE",
                schema: "identity",
                table: "users",
                columns: new[] { "TenantId", "Role", "IsActive" });
        }
    }
}
