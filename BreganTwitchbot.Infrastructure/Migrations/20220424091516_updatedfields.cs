using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    public partial class updatedfields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_points",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Config",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "channelPointsRewardCost",
                table: "StreamStats");

            migrationBuilder.DropColumn(
                name: "channelPointsRewardsRedeemed",
                table: "StreamStats");

            migrationBuilder.DropColumn(
                name: "totalLostStreaks",
                table: "StreamStats");

            migrationBuilder.AddColumn<string>(
                name: "broadcasterName",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Config",
                table: "Config",
                column: "broadcasterName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Config",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "broadcasterName",
                table: "Config");

            migrationBuilder.AddColumn<long>(
                name: "channelPointsRewardCost",
                table: "StreamStats",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "channelPointsRewardsRedeemed",
                table: "StreamStats",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "totalLostStreaks",
                table: "StreamStats",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Config",
                table: "Config",
                column: "broadcasterOAuth");

            migrationBuilder.CreateIndex(
                name: "IX_Users_points",
                table: "Users",
                column: "points");
        }
    }
}