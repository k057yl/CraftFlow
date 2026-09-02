using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "warehouses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "units_of_measure",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "procurement",
                table: "suppliers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "stock_lots",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "sales_orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "sales_order_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "recipes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "recipe_ingredients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "raw_materials",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "procurement",
                table: "purchase_orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "procurement",
                table: "purchase_order_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "production_batches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "consumed_ingredients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "public",
                table: "audit_logs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "aging",
                table: "aging_lots",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "aging",
                table: "aging_chambers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "warehouses");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "units_of_measure");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "procurement",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "stock_lots");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "sales_orders");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "sales_order_items");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "recipes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "recipe_ingredients");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "raw_materials");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "procurement",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "procurement",
                table: "purchase_order_items");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "products");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "production_batches");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "consumed_ingredients");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "public",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "aging",
                table: "aging_lots");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "aging",
                table: "aging_chambers");
        }
    }
}
