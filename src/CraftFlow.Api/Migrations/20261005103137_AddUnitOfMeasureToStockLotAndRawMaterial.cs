using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitOfMeasureToStockLotAndRawMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "catalog",
                table: "units_of_measure",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "ConversionFactor",
                schema: "catalog",
                table: "units_of_measure",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "catalog",
                table: "units_of_measure",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "UnitOfMeasureId",
                schema: "inventory",
                table: "stock_lots",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "catalog",
                table: "raw_materials",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_units_of_measure_BaseUnitId",
                schema: "catalog",
                table: "units_of_measure",
                column: "BaseUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_lots_UnitOfMeasureId",
                schema: "inventory",
                table: "stock_lots",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_raw_materials_UnitOfMeasureId",
                schema: "catalog",
                table: "raw_materials",
                column: "UnitOfMeasureId");

            migrationBuilder.AddForeignKey(
                name: "FK_raw_materials_units_of_measure_UnitOfMeasureId",
                schema: "catalog",
                table: "raw_materials",
                column: "UnitOfMeasureId",
                principalSchema: "catalog",
                principalTable: "units_of_measure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_lots_units_of_measure_UnitOfMeasureId",
                schema: "inventory",
                table: "stock_lots",
                column: "UnitOfMeasureId",
                principalSchema: "catalog",
                principalTable: "units_of_measure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_units_of_measure_units_of_measure_BaseUnitId",
                schema: "catalog",
                table: "units_of_measure",
                column: "BaseUnitId",
                principalSchema: "catalog",
                principalTable: "units_of_measure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_raw_materials_units_of_measure_UnitOfMeasureId",
                schema: "catalog",
                table: "raw_materials");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_lots_units_of_measure_UnitOfMeasureId",
                schema: "inventory",
                table: "stock_lots");

            migrationBuilder.DropForeignKey(
                name: "FK_units_of_measure_units_of_measure_BaseUnitId",
                schema: "catalog",
                table: "units_of_measure");

            migrationBuilder.DropIndex(
                name: "IX_units_of_measure_BaseUnitId",
                schema: "catalog",
                table: "units_of_measure");

            migrationBuilder.DropIndex(
                name: "IX_stock_lots_UnitOfMeasureId",
                schema: "inventory",
                table: "stock_lots");

            migrationBuilder.DropIndex(
                name: "IX_raw_materials_UnitOfMeasureId",
                schema: "catalog",
                table: "raw_materials");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureId",
                schema: "inventory",
                table: "stock_lots");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "catalog",
                table: "units_of_measure",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<decimal>(
                name: "ConversionFactor",
                schema: "catalog",
                table: "units_of_measure",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "catalog",
                table: "units_of_measure",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "catalog",
                table: "raw_materials",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);
        }
    }
}
