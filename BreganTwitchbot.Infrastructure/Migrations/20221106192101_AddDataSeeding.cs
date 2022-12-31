using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    public partial class AddDataSeeding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Config",
                columns: new[] { "BroadcasterName", "BotName", "BotOAuth", "BroadcasterOAuth", "BroadcasterRefresh", "DailyPointsCollectingAllowed", "DiscordAPIKey", "DiscordBanRole", "DiscordCommandsChannelID", "DiscordEventChannelID", "DiscordGeneralChannel", "DiscordGiveawayChannelID", "DiscordGuildID", "DiscordGuildOwner", "DiscordLinkingChannelID", "DiscordRankUpAnnouncementChannelID", "DiscordReactionRoleChannelID", "DiscordSocksChannelID", "DiscordStreamAnnouncementChannelID", "LastDailyPointsAllowed", "PinnedStreamDate", "PinnedStreamMessage", "PinnedStreamMessageId", "PointsName", "PrestigeCap", "StreamAnnounced", "SubathonTime", "TwitchAPIClientID", "TwitchAPISecret", "TwitchChannelID" },
                values: new object[] { "", "", "", "", "", false, "", 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, new DateTime(2022, 11, 5, 19, 21, 1, 156, DateTimeKind.Utc).AddTicks(8214), new DateTime(2022, 11, 5, 19, 21, 1, 156, DateTimeKind.Utc).AddTicks(8200), "", 0m, "", 0L, false, new TimeSpan(0, 0, 0, 0, 0), "", "", "" });

            migrationBuilder.InsertData(
                table: "SlotMachine",
                columns: new[] { "StreamName", "CheeseWins", "CherriesWins", "CucumberWins", "DiscordTotalSpins", "EggplantWins", "GrapesWins", "JackpotAmount", "JackpotWins", "PineappleWins", "SmorcWins", "Tier1Wins", "Tier2Wins", "Tier3Wins", "TotalSpins" },
                values: new object[] { "", 0, 0, 0, 0, 0, 0, 0L, 0, 0, 0, 0, 0, 0, 0 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "");

            migrationBuilder.DeleteData(
                table: "SlotMachine",
                keyColumn: "StreamName",
                keyValue: "");
        }
    }
}