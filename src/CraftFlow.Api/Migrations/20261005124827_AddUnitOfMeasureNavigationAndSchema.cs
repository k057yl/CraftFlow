using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitOfMeasureNavigationAndSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_products_UnitOfMeasureId",
                schema: "catalog",
                table: "products",
                column: "UnitOfMeasureId");

            migrationBuilder.AddForeignKey(
                name: "FK_products_units_of_measure_UnitOfMeasureId",
                schema: "catalog",
                table: "products",
                column: "UnitOfMeasureId",
                principalSchema: "catalog",
                principalTable: "units_of_measure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_units_of_measure_UnitOfMeasureId",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_UnitOfMeasureId",
                schema: "catalog",
                table: "products");
        }
    }
}
