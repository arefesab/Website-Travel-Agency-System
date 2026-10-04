using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace agancywebProject.Migrations
{
    public partial class AddFlightClassAndHotelRoomFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "flightClass",
                table: "Flight",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "اکونومی");

            migrationBuilder.AddColumn<string>(
                name: "roomName",
                table: "Hotel",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "اتاق استاندارد");

            migrationBuilder.AddColumn<int>(
                name: "roomCapacity",
                table: "Hotel",
                type: "int",
                nullable: false,
                defaultValue: 2);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "flightClass",
                table: "Flight");

            migrationBuilder.DropColumn(
                name: "roomName",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "roomCapacity",
                table: "Hotel");
        }
    }
}
