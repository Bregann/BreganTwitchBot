using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStreamHappenedThisWeekField : Migration
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
                columns: new[] { "LastDailyPointsAllowed", "PinnedStreamDate", "StreamHappenedThisWeek" },
                values: new object[] { new DateTime(2023, 12, 29, 19, 36, 57, 63, DateTimeKind.Utc).AddTicks(8889), new DateTime(2023, 12, 29, 19, 36, 57, 63, DateTimeKind.Utc).AddTicks(8884), false });
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
                columns: new[] { "LastDailyPointsAllowed", "PinnedStreamDate" },
                values: new object[] { new DateTime(2023, 12, 1, 14, 8, 25, 634, DateTimeKind.Utc).AddTicks(572), new DateTime(2023, 12, 1, 14, 8, 25, 634, DateTimeKind.Utc).AddTicks(563) });
        }
    }
}
