using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace agancywebProject.Migrations
{
    public partial class UpdateFlightAndHotelFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "finishdate",
                table: "Flight");

            migrationBuilder.DropColumn(
                name: "exittime",
                table: "Flight");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "Hotel",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "photoUrls",
                table: "Hotel",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "photoUrls",
                table: "Hotel");

            migrationBuilder.AddColumn<DateTime>(
                name: "finishdate",
                table: "Flight",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "exittime",
                table: "Flight",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
