using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseTypeAndStorageLocationEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                schema: "inventory",
                table: "warehouses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                schema: "inventory",
                table: "warehouses");
        }
    }
}
