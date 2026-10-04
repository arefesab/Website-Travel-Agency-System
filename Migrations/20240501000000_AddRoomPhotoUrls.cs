using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using agancywebProject.Data;

#nullable disable

namespace agancywebProject.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240501000000_AddRoomPhotoUrls")]
    public partial class AddRoomPhotoUrls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // photoUrls keeps the existing (hotel) photos; roomPhotoUrls is new and starts empty.
            migrationBuilder.AddColumn<string>(
                name: "roomPhotoUrls",
                table: "Hotel",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "roomPhotoUrls",
                table: "Hotel");
        }
    }
}
