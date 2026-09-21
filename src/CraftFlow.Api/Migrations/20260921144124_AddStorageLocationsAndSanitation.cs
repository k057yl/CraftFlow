using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStorageLocationsAndSanitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOccupied",
                schema: "public",
                table: "storage_locations");

            migrationBuilder.AddColumn<int>(
                name: "BatchesProcessedCount",
                schema: "public",
                table: "storage_locations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentVolume",
                schema: "public",
                table: "storage_locations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWashedAt",
                schema: "public",
                table: "storage_locations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextWashDueDate",
                schema: "public",
                table: "storage_locations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WashCycleBatchInterval",
                schema: "public",
                table: "storage_locations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatchesProcessedCount",
                schema: "public",
                table: "storage_locations");

            migrationBuilder.DropColumn(
                name: "CurrentVolume",
                schema: "public",
                table: "storage_locations");

            migrationBuilder.DropColumn(
                name: "LastWashedAt",
                schema: "public",
                table: "storage_locations");

            migrationBuilder.DropColumn(
                name: "NextWashDueDate",
                schema: "public",
                table: "storage_locations");

            migrationBuilder.DropColumn(
                name: "WashCycleBatchInterval",
                schema: "public",
                table: "storage_locations");

            migrationBuilder.AddColumn<bool>(
                name: "IsOccupied",
                schema: "public",
                table: "storage_locations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
