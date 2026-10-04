using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace agancywebProject.Migrations
{
    public partial class AddHotelDiscountPercent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Booking_Hotel_Id",
                table: "Booking");

            migrationBuilder.AddColumn<int>(
                name: "DiscountPercent",
                table: "Hotel",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Hotel");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Hotel_Id",
                table: "Booking",
                column: "Hotel_Id");
        }
    }
}
