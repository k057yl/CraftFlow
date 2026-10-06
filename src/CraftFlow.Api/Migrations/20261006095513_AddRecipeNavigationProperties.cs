using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_recipes_ProductId",
                schema: "catalog",
                table: "recipes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_ingredients_RawMaterialId",
                schema: "catalog",
                table: "recipe_ingredients",
                column: "RawMaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_recipe_ingredients_raw_materials_RawMaterialId",
                schema: "catalog",
                table: "recipe_ingredients",
                column: "RawMaterialId",
                principalSchema: "catalog",
                principalTable: "raw_materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_recipes_products_ProductId",
                schema: "catalog",
                table: "recipes",
                column: "ProductId",
                principalSchema: "catalog",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_recipe_ingredients_raw_materials_RawMaterialId",
                schema: "catalog",
                table: "recipe_ingredients");

            migrationBuilder.DropForeignKey(
                name: "FK_recipes_products_ProductId",
                schema: "catalog",
                table: "recipes");

            migrationBuilder.DropIndex(
                name: "IX_recipes_ProductId",
                schema: "catalog",
                table: "recipes");

            migrationBuilder.DropIndex(
                name: "IX_recipe_ingredients_RawMaterialId",
                schema: "catalog",
                table: "recipe_ingredients");
        }
    }
}
