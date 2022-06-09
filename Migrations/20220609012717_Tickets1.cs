using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2.Migrations
{
    public partial class Tickets1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DepartureDate",
                table: "Tickets",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartureDate",
                table: "Tickets");
        }
    }
}
