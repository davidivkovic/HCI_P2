using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2.Migrations
{
    public partial class MoveSeats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seat_Trains_TrainId",
                table: "Seat");

            migrationBuilder.DropTable(
                name: "Slot");

            migrationBuilder.RenameColumn(
                name: "TrainId",
                table: "Seat",
                newName: "SeatGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_Seat_TrainId",
                table: "Seat",
                newName: "IX_Seat_SeatGroupId");

            migrationBuilder.AddColumn<int>(
                name: "Col",
                table: "Seat",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Row",
                table: "Seat",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SeatNumber",
                table: "Seat",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SeatGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SeatType = table.Column<int>(type: "INTEGER", nullable: false),
                    TrainId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeatGroup_Trains_TrainId",
                        column: x => x.TrainId,
                        principalTable: "Trains",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeatGroup_TrainId",
                table: "SeatGroup",
                column: "TrainId");

            migrationBuilder.AddForeignKey(
                name: "FK_Seat_SeatGroup_SeatGroupId",
                table: "Seat",
                column: "SeatGroupId",
                principalTable: "SeatGroup",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seat_SeatGroup_SeatGroupId",
                table: "Seat");

            migrationBuilder.DropTable(
                name: "SeatGroup");

            migrationBuilder.DropColumn(
                name: "Col",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "Row",
                table: "Seat");

            migrationBuilder.DropColumn(
                name: "SeatNumber",
                table: "Seat");

            migrationBuilder.RenameColumn(
                name: "SeatGroupId",
                table: "Seat",
                newName: "TrainId");

            migrationBuilder.RenameIndex(
                name: "IX_Seat_SeatGroupId",
                table: "Seat",
                newName: "IX_Seat_TrainId");

            migrationBuilder.CreateTable(
                name: "Slot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Col = table.Column<int>(type: "INTEGER", nullable: false),
                    Row = table.Column<int>(type: "INTEGER", nullable: false),
                    SeatId = table.Column<int>(type: "INTEGER", nullable: true),
                    SeatNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    SeatType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Slot_Seat_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seat",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Slot_SeatId",
                table: "Slot",
                column: "SeatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Seat_Trains_TrainId",
                table: "Seat",
                column: "TrainId",
                principalTable: "Trains",
                principalColumn: "Id");
        }
    }
}
