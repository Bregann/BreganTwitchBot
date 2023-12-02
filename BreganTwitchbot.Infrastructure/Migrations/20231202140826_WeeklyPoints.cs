using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WeeklyPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentMonthlyStreak",
                schema: "twitchbot_new",
                table: "DailyPoints",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentWeeklyStreak",
                schema: "twitchbot_new",
                table: "DailyPoints",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentYearlyStreak",
                schema: "twitchbot_new",
                table: "DailyPoints",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HighestMonthlyStreak",
                schema: "twitchbot_new",
                table: "DailyPoints",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HighestWeeklyStreak",
                schema: "twitchbot_new",
                table: "DailyPoints",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HighestYearlyStreak",
                schema: "twitchbot_new",
                table: "DailyPoints",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "MonthlyClaimed",
                schema: "twitchbot_new",
                table: "DailyPoints",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TotalMonthlyClaimed",
                schema: "twitchbot_new",
                table: "DailyPoints",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWeeklyClaimed",
                schema: "twitchbot_new",
                table: "DailyPoints",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalYearlyClaimed",
                schema: "twitchbot_new",
                table: "DailyPoints",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "WeeklyClaimed",
                schema: "twitchbot_new",
                table: "DailyPoints",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "YearlyClaimed",
                schema: "twitchbot_new",
                table: "DailyPoints",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "twitchbot_new",
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                columns: new[] { "LastDailyPointsAllowed", "PinnedStreamDate" },
                values: new object[] { new DateTime(2023, 12, 1, 14, 8, 25, 634, DateTimeKind.Utc).AddTicks(572), new DateTime(2023, 12, 1, 14, 8, 25, 634, DateTimeKind.Utc).AddTicks(563) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentMonthlyStreak",
                schema: "twitchbot_new",
                table: "DailyPoints");

            migrationBuilder.DropColumn(
                name: "CurrentWeeklyStreak",
                schema: "twitchbot_new",
                table: "DailyPoints");

            migrationBuilder.DropColumn(
                name: "CurrentYearlyStreak",
                schema: "twitchbot_new",
                table: "DailyPoints");

            migrationBuilder.DropColumn(
                name: "HighestMonthlyStreak",
                schema: "twitchbot_new",
                table: "DailyPoints");

            migrationBuilder.DropColumn(
                name: "HighestWeeklyStreak",
                schema: "twitchbot_new",
                table: "DailyPoints");

            migrationBuilder.DropColumn(
                name: "HighestYearlyStreak",
                schema: "twitchbot_new",
                table: "DailyPoints");

            migrationBuilder.DropColumn(
                name: "MonthlyClaimed",
                schema: "twitchbot_new",
                table: "DailyPoints");

            migrationBuilder.DropColumn(
                name: "TotalMonthlyClaimed",
                schema: "twitchbot_new",
                table: "DailyPoints");

            migrationBuilder.DropColumn(
                name: "TotalWeeklyClaimed",
                schema: "twitchbot_new",
                table: "DailyPoints");

            migrationBuilder.DropColumn(
                name: "TotalYearlyClaimed",
                schema: "twitchbot_new",
                table: "DailyPoints");

            migrationBuilder.DropColumn(
                name: "WeeklyClaimed",
                schema: "twitchbot_new",
                table: "DailyPoints");

            migrationBuilder.DropColumn(
                name: "YearlyClaimed",
                schema: "twitchbot_new",
                table: "DailyPoints");

            migrationBuilder.UpdateData(
                schema: "twitchbot_new",
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                columns: new[] { "LastDailyPointsAllowed", "PinnedStreamDate" },
                values: new object[] { new DateTime(2023, 3, 17, 14, 32, 30, 332, DateTimeKind.Utc).AddTicks(2093), new DateTime(2023, 3, 17, 14, 32, 30, 332, DateTimeKind.Utc).AddTicks(2087) });
        }
    }
}
