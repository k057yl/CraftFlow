using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraftFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameAgingStatusToAgingState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "aging",
                table: "aging_lots",
                newName: "State");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "State",
                schema: "aging",
                table: "aging_lots",
                newName: "Status");
        }
    }
}
