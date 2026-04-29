using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PitStop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerResponseToReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerResponse",
                table: "Reviews",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OwnerResponseAt",
                table: "Reviews",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerResponse",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "OwnerResponseAt",
                table: "Reviews");
        }
    }
}
