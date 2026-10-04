using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace agancywebProject.Migrations
{
    public partial class MakePricesIntAndAddFlightBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "price",
                table: "Hotel",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "price",
                table: "Flight",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "TotalPrice",
                table: "Booking",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "FlightBooking",
                columns: table => new
                {
                    FlightBooking_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Flight_Id = table.Column<int>(type: "int", nullable: false),
                    SeatCount = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TotalPrice = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Authority = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RefId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightBooking", x => x.FlightBooking_Id);
                    table.ForeignKey(
                        name: "FK_FlightBooking_Flight_Flight_Id",
                        column: x => x.Flight_Id,
                        principalTable: "Flight",
                        principalColumn: "Flight_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlightBooking_Flight_Id",
                table: "FlightBooking",
                column: "Flight_Id");

            migrationBuilder.CreateIndex(
                name: "IX_FlightBooking_Authority",
                table: "FlightBooking",
                column: "Authority");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlightBooking");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "Booking",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "Flight",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "Hotel",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
