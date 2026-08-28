using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDestinationWarehouseToProductionBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DestinationWarehouseId",
                table: "production_batches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationWarehouseId",
                table: "production_batches");
        }
    }
}
