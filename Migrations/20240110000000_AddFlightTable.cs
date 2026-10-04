using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace agancywebProject.Migrations
{
    public partial class AddFlightTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Flight",
                columns: table => new
                {
                    Flight_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Origin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    distination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    startdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    finishdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    capacity = table.Column<int>(type: "int", nullable: false),
                    entrytime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    exittime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flight", x => x.Flight_Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Flight");
        }
    }
}
