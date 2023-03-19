using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHangfireDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HangfirePassword",
                schema: "twitchbot_new",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HangfireUsername",
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
                columns: new[] { "HangfirePassword", "HangfireUsername", "LastDailyPointsAllowed", "PinnedStreamDate" },
                values: new object[] { "", "", new DateTime(2023, 3, 17, 14, 32, 30, 332, DateTimeKind.Utc).AddTicks(2093), new DateTime(2023, 3, 17, 14, 32, 30, 332, DateTimeKind.Utc).AddTicks(2087) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HangfirePassword",
                schema: "twitchbot_new",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "HangfireUsername",
                schema: "twitchbot_new",
                table: "Config");

            migrationBuilder.UpdateData(
                schema: "twitchbot_new",
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                columns: new[] { "LastDailyPointsAllowed", "PinnedStreamDate" },
                values: new object[] { new DateTime(2023, 3, 17, 14, 3, 38, 354, DateTimeKind.Utc).AddTicks(7752), new DateTime(2023, 3, 17, 14, 3, 38, 354, DateTimeKind.Utc).AddTicks(7748) });
        }
    }
}
