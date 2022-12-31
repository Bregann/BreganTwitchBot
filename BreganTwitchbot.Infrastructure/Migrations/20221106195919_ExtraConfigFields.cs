using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    public partial class ExtraConfigFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HFConnectionString",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectMonitorApiKey",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                columns: new[] { "HFConnectionString", "LastDailyPointsAllowed", "PinnedStreamDate", "ProjectMonitorApiKey" },
                values: new object[] { "", new DateTime(2022, 11, 5, 19, 59, 18, 876, DateTimeKind.Utc).AddTicks(1979), new DateTime(2022, 11, 5, 19, 59, 18, 876, DateTimeKind.Utc).AddTicks(1973), "" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HFConnectionString",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "ProjectMonitorApiKey",
                table: "Config");

            migrationBuilder.UpdateData(
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                columns: new[] { "LastDailyPointsAllowed", "PinnedStreamDate" },
                values: new object[] { new DateTime(2022, 11, 5, 19, 21, 1, 156, DateTimeKind.Utc).AddTicks(8214), new DateTime(2022, 11, 5, 19, 21, 1, 156, DateTimeKind.Utc).AddTicks(8200) });
        }
    }
}