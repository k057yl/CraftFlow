using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionBatchTimers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetDurationMinutes",
                schema: "public",
                table: "recipes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "AgingStartedAt",
                schema: "public",
                table: "production_batches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BrewingCompletedAt",
                schema: "public",
                table: "production_batches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTelegramNotified",
                schema: "public",
                table: "production_batches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TargetDurationMinutes",
                schema: "public",
                table: "production_batches",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetDurationMinutes",
                schema: "public",
                table: "recipes");

            migrationBuilder.DropColumn(
                name: "AgingStartedAt",
                schema: "public",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "BrewingCompletedAt",
                schema: "public",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "IsTelegramNotified",
                schema: "public",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "TargetDurationMinutes",
                schema: "public",
                table: "production_batches");
        }
    }
}
