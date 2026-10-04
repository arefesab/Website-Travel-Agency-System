using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace agancywebProject.Migrations
{
    public partial class AddDateRangeBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBooked",
                table: "Hotel");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckIn",
                table: "Booking",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1900, 1, 1));

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOut",
                table: "Booking",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1900, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Hotel_Id_CheckIn_CheckOut",
                table: "Booking",
                columns: new[] { "Hotel_Id", "CheckIn", "CheckOut" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Booking_Hotel_Id_CheckIn_CheckOut",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "CheckIn",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "CheckOut",
                table: "Booking");

            migrationBuilder.AddColumn<bool>(
                name: "IsBooked",
                table: "Hotel",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
