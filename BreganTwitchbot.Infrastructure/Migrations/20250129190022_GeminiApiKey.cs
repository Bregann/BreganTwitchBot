using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GeminiApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeminiApiKey",
                schema: "twitchbot_new",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                schema: "twitchbot_new",
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                columns: new[] { "GeminiApiKey", "LastDailyPointsAllowed", "PinnedStreamDate" },
                values: new object[] { "", new DateTime(2025, 1, 28, 19, 0, 21, 808, DateTimeKind.Utc).AddTicks(2659), new DateTime(2025, 1, 28, 19, 0, 21, 808, DateTimeKind.Utc).AddTicks(2652) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeminiApiKey",
                schema: "twitchbot_new",
                table: "Config");

            migrationBuilder.UpdateData(
                schema: "twitchbot_new",
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                columns: new[] { "LastDailyPointsAllowed", "PinnedStreamDate" },
                values: new object[] { new DateTime(2025, 1, 27, 19, 52, 6, 926, DateTimeKind.Utc).AddTicks(7659), new DateTime(2025, 1, 27, 19, 52, 6, 926, DateTimeKind.Utc).AddTicks(7655) });
        }
    }
}
