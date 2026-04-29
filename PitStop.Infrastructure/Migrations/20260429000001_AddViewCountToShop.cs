using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PitStop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddViewCountToShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Shops",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Shops");
        }
    }
}
