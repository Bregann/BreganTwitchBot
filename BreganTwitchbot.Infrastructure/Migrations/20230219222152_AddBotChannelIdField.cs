using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBotChannelIdField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BotChannelId",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                columns: new[] { "BotChannelId", "LastDailyPointsAllowed", "PinnedStreamDate" },
                values: new object[] { "", new DateTime(2023, 2, 18, 22, 21, 52, 422, DateTimeKind.Utc).AddTicks(2158), new DateTime(2023, 2, 18, 22, 21, 52, 422, DateTimeKind.Utc).AddTicks(2154) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BotChannelId",
                table: "Config");

            migrationBuilder.UpdateData(
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                columns: new[] { "LastDailyPointsAllowed", "PinnedStreamDate" },
                values: new object[] { new DateTime(2023, 2, 10, 22, 41, 34, 759, DateTimeKind.Utc).AddTicks(8345), new DateTime(2023, 2, 10, 22, 41, 34, 759, DateTimeKind.Utc).AddTicks(8341) });
        }
    }
}
