using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeNavigationAndCatalogIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "FK_recipe_ingredients_recipes_RecipeId1",
                schema: "catalog",
                table: "recipe_ingredients",
                column: "RecipeId1",
                principalSchema: "catalog",
                principalTable: "recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
