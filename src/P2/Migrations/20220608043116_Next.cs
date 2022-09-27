using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2.Migrations
{
    public partial class Next : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stop_Lines_TrainLineId",
                table: "Stop");

            migrationBuilder.DropForeignKey(
                name: "FK_Stop_Stations_StationId",
                table: "Stop");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Stop",
                table: "Stop");

            migrationBuilder.RenameTable(
                name: "Stop",
                newName: "Stops");

            migrationBuilder.RenameIndex(
                name: "IX_Stop_TrainLineId",
                table: "Stops",
                newName: "IX_Stops_TrainLineId");

            migrationBuilder.RenameIndex(
                name: "IX_Stop_StationId",
                table: "Stops",
                newName: "IX_Stops_StationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stops",
                table: "Stops",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stops_Lines_TrainLineId",
                table: "Stops",
                column: "TrainLineId",
                principalTable: "Lines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stops_Stations_StationId",
                table: "Stops",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stops_Lines_TrainLineId",
                table: "Stops");

            migrationBuilder.DropForeignKey(
                name: "FK_Stops_Stations_StationId",
                table: "Stops");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Stops",
                table: "Stops");

            migrationBuilder.RenameTable(
                name: "Stops",
                newName: "Stop");

            migrationBuilder.RenameIndex(
                name: "IX_Stops_TrainLineId",
                table: "Stop",
                newName: "IX_Stop_TrainLineId");

            migrationBuilder.RenameIndex(
                name: "IX_Stops_StationId",
                table: "Stop",
                newName: "IX_Stop_StationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stop",
                table: "Stop",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stop_Lines_TrainLineId",
                table: "Stop",
                column: "TrainLineId",
                principalTable: "Lines",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Stop_Stations_StationId",
                table: "Stop",
                column: "StationId",
                principalTable: "Stations",
                principalColumn: "Id");
        }
    }
}
