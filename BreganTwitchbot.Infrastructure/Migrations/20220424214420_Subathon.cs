using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    public partial class Subathon : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "subathonTime",
                table: "Config",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.CreateTable(
                name: "Subathon",
                columns: table => new
                {
                    username = table.Column<string>(type: "text", nullable: false),
                    subsGifted = table.Column<int>(type: "integer", nullable: false),
                    bitsDonated = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subathon", x => x.username);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subathon");

            migrationBuilder.DropColumn(
                name: "subathonTime",
                table: "Config");
        }
    }
}