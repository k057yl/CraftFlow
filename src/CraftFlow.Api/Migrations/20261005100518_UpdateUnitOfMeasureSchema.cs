using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUnitOfMeasureSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BaseUnitId",
                schema: "catalog",
                table: "units_of_measure",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ConversionFactor",
                schema: "catalog",
                table: "units_of_measure",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "catalog",
                table: "units_of_measure",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseUnitId",
                schema: "catalog",
                table: "units_of_measure");

            migrationBuilder.DropColumn(
                name: "ConversionFactor",
                schema: "catalog",
                table: "units_of_measure");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "catalog",
                table: "units_of_measure");
        }
    }
}
