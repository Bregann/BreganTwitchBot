using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBotKeyAndRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TwitchBotApiKey",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TwitchBotApiRefresh",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                columns: new[] { "LastDailyPointsAllowed", "PinnedStreamDate", "TwitchBotApiKey", "TwitchBotApiRefresh" },
                values: new object[] { new DateTime(2023, 2, 10, 22, 41, 34, 759, DateTimeKind.Utc).AddTicks(8345), new DateTime(2023, 2, 10, 22, 41, 34, 759, DateTimeKind.Utc).AddTicks(8341), "", "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TwitchBotApiKey",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "TwitchBotApiRefresh",
                table: "Config");

            migrationBuilder.UpdateData(
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                columns: new[] { "LastDailyPointsAllowed", "PinnedStreamDate" },
                values: new object[] { new DateTime(2022, 11, 5, 19, 59, 18, 876, DateTimeKind.Utc).AddTicks(1979), new DateTime(2022, 11, 5, 19, 59, 18, 876, DateTimeKind.Utc).AddTicks(1973) });
        }
    }
}
