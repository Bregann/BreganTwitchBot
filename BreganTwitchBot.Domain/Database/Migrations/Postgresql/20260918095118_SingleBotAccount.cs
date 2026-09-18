using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Domain.Database.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class SingleBotAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BotTwitchChannelId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "BotTwitchChannelName",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "BotTwitchChannelOAuthToken",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "BotTwitchChannelRefreshToken",
                table: "Channels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BotTwitchChannelId",
                table: "Channels",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BotTwitchChannelName",
                table: "Channels",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BotTwitchChannelOAuthToken",
                table: "Channels",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BotTwitchChannelRefreshToken",
                table: "Channels",
                type: "text",
                nullable: false,
                defaultValue: "");

        }
    }
}
