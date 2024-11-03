using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StreamHappenedThing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StreamHappenedThisWeek",
                schema: "twitchbot_new",
                table: "Config",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "twitchbot_new",
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                columns: new[] { "LastDailyPointsAllowed", "StreamHappenedThisWeek" },
                values: new object[] { new DateTime(2024, 10, 13, 19, 33, 13, 263, DateTimeKind.Utc).AddTicks(9236), false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StreamHappenedThisWeek",
                schema: "twitchbot_new",
                table: "Config");

            migrationBuilder.UpdateData(
                schema: "twitchbot_new",
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                column: "LastDailyPointsAllowed",
                value: new DateTime(2023, 12, 1, 14, 21, 35, 48, DateTimeKind.Utc).AddTicks(4440));
        }
    }
}
