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
            // There is now a single bot account across every channel rather than one per channel.
            // Carry the existing bot credentials over into the environmental settings before the
            // per channel columns are dropped, taking them from the lowest channel id. Any other
            // bot accounts are no longer used and are intentionally discarded.
            migrationBuilder.Sql(@"
                INSERT INTO ""EnvironmentalSettings"" (""Key"", ""Value"")
                SELECT s.key, s.value
                FROM (
                    SELECT
                        'BotTwitchChannelId' AS key, ""BotTwitchChannelId"" AS value FROM ""Channels"" ORDER BY ""Id"" LIMIT 1
                    UNION ALL
                    SELECT
                        'BotTwitchChannelName', ""BotTwitchChannelName"" FROM ""Channels"" ORDER BY ""Id"" LIMIT 1
                    UNION ALL
                    SELECT
                        'BotTwitchChannelOAuthToken', ""BotTwitchChannelOAuthToken"" FROM ""Channels"" ORDER BY ""Id"" LIMIT 1
                    UNION ALL
                    SELECT
                        'BotTwitchChannelRefreshToken', ""BotTwitchChannelRefreshToken"" FROM ""Channels"" ORDER BY ""Id"" LIMIT 1
                ) s
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""EnvironmentalSettings"" e WHERE e.""Key"" = s.key
                );
            ");

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

            // Put the single bot account back onto every channel, then drop the settings rows
            migrationBuilder.Sql(@"
                UPDATE ""Channels"" SET
                    ""BotTwitchChannelId"" = COALESCE((SELECT ""Value"" FROM ""EnvironmentalSettings"" WHERE ""Key"" = 'BotTwitchChannelId'), ''),
                    ""BotTwitchChannelName"" = COALESCE((SELECT ""Value"" FROM ""EnvironmentalSettings"" WHERE ""Key"" = 'BotTwitchChannelName'), ''),
                    ""BotTwitchChannelOAuthToken"" = COALESCE((SELECT ""Value"" FROM ""EnvironmentalSettings"" WHERE ""Key"" = 'BotTwitchChannelOAuthToken'), ''),
                    ""BotTwitchChannelRefreshToken"" = COALESCE((SELECT ""Value"" FROM ""EnvironmentalSettings"" WHERE ""Key"" = 'BotTwitchChannelRefreshToken'), '');

                DELETE FROM ""EnvironmentalSettings"" WHERE ""Key"" IN (
                    'BotTwitchChannelId', 'BotTwitchChannelName', 'BotTwitchChannelOAuthToken', 'BotTwitchChannelRefreshToken'
                );
            ");
        }
    }
}
