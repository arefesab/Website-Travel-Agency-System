using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace agancywebProject.Migrations
{
    public partial class AddBookingSystemAndHotelExtras : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBooked",
                table: "Hotel",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "amenities",
                table: "Hotel",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    Booking_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Hotel_Id = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nights = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Authority = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RefId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking", x => x.Booking_Id);
                    table.ForeignKey(
                        name: "FK_Booking_Hotel_Hotel_Id",
                        column: x => x.Hotel_Id,
                        principalTable: "Hotel",
                        principalColumn: "Hotel_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Hotel_Id",
                table: "Booking",
                column: "Hotel_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Authority",
                table: "Booking",
                column: "Authority");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropColumn(
                name: "IsBooked",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "amenities",
                table: "Hotel");
        }
    }
}
