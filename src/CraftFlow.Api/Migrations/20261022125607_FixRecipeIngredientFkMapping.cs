using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixRecipeIngredientFkMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_recipe_ingredients_raw_materials_RawMaterialId",
                schema: "catalog",
                table: "recipe_ingredients");

            migrationBuilder.DropForeignKey(
                name: "FK_recipe_ingredients_recipes_RecipeId1",
                schema: "catalog",
                table: "recipe_ingredients");

            migrationBuilder.DropIndex(
                name: "IX_recipe_ingredients_RecipeId1",
                schema: "catalog",
                table: "recipe_ingredients");

            migrationBuilder.DropColumn(
                name: "RecipeId1",
                schema: "catalog",
                table: "recipe_ingredients");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                schema: "catalog",
                table: "recipe_ingredients",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddForeignKey(
                name: "FK_recipe_ingredients_raw_materials_RawMaterialId",
                schema: "catalog",
                table: "recipe_ingredients",
                column: "RawMaterialId",
                principalSchema: "catalog",
                principalTable: "raw_materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_recipe_ingredients_raw_materials_RawMaterialId",
                schema: "catalog",
                table: "recipe_ingredients");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                schema: "catalog",
                table: "recipe_ingredients",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddColumn<Guid>(
                name: "RecipeId1",
                schema: "catalog",
                table: "recipe_ingredients",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_recipe_ingredients_RecipeId1",
                schema: "catalog",
                table: "recipe_ingredients",
                column: "RecipeId1");

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
                name: "FK_recipe_ingredients_recipes_RecipeId1",
                schema: "catalog",
                table: "recipe_ingredients",
                column: "RecipeId1",
                principalSchema: "catalog",
                principalTable: "recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
