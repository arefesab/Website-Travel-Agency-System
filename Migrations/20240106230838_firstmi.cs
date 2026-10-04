using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace agancywebProject.Migrations
{
    public partial class firstmi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "Hotel",
                columns: table => new
                {
                    Hotel_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    city = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    startdate = table.Column<TimeSpan>(type: "time", nullable: false),
                    finishdate = table.Column<TimeSpan>(type: "time", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    star = table.Column<int>(type: "int", nullable: false),
                    meal = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotel", x => x.Hotel_Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "Hotel");
        }
    }
}
