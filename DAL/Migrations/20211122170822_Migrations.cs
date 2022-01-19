using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class Migrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameOptions",
                columns: table => new
                {
                    GameOptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoardSize = table.Column<int>(type: "int", nullable: false),
                    MaxShipSize = table.Column<int>(type: "int", nullable: false),
                    ShipAmount = table.Column<int>(type: "int", nullable: false),
                    TouchState = table.Column<int>(type: "int", nullable: false),
                    Combo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameOptions", x => x.GameOptionId);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameOptionId = table.Column<int>(type: "int", nullable: false),
                    BoardData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.GameId);
                    table.ForeignKey(
                        name: "FK_Games_GameOptions_GameOptionId",
                        column: x => x.GameOptionId,
                        principalTable: "GameOptions",
                        principalColumn: "GameOptionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_GameOptionId",
                table: "Games",
                column: "GameOptionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "GameOptions");
        }
    }
}
