using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCleanDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenant_access_keys",
                schema: "saas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenant_access_keys",
                schema: "saas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    KeyHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    KeyPrefix = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    KeySuffix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    LastUsedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_access_keys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_access_keys_tenant_subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalSchema: "saas",
                        principalTable: "tenant_subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ACCESS_KEYS_HASH",
                schema: "saas",
                table: "tenant_access_keys",
                column: "KeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ACCESS_KEYS_TENANT",
                schema: "saas",
                table: "tenant_access_keys",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_access_keys_SubscriptionId",
                schema: "saas",
                table: "tenant_access_keys",
                column: "SubscriptionId");
        }
    }
}
