using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2.Migrations
{
    public partial class SeatSlots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Seating");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Trains");

            migrationBuilder.AddColumn<int>(
                name: "TrainId",
                table: "Departures",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Seat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrainId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seat_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Slot",
                columns: table => new
                {
                    SeatId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Row = table.Column<int>(type: "INTEGER", nullable: false),
                    Col = table.Column<int>(type: "INTEGER", nullable: false),
                    SeatNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    SeatType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slot", x => new { x.SeatId, x.Id });
                    table.ForeignKey(
                        name: "FK_Slot_Seat_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departures_TrainId",
                table: "Departures",
                column: "TrainId");

            migrationBuilder.CreateIndex(
                name: "IX_Seat_TrainId",
                table: "Seat",
                column: "TrainId");

            migrationBuilder.AddForeignKey(
                name: "FK_Departures_Trains_TrainId",
                table: "Departures",
                column: "TrainId",
                principalTable: "Trains",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departures_Trains_TrainId",
                table: "Departures");

            migrationBuilder.DropTable(
                name: "Slot");

            migrationBuilder.DropTable(
                name: "Seat");

            migrationBuilder.DropIndex(
                name: "IX_Departures_TrainId",
                table: "Departures");

            migrationBuilder.DropColumn(
                name: "TrainId",
                table: "Departures");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Trains",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Seating",
                columns: table => new
                {
                    TrainId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Rows = table.Column<int>(type: "INTEGER", nullable: false),
                    SeatsPerRow = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seating", x => new { x.TrainId, x.Id });
                    table.ForeignKey(
                        name: "FK_Seating_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
