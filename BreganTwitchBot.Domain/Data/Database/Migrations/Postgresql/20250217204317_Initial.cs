using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BreganTwitchBot.Domain.Data.Database.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChannelUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TwitchUserId = table.Column<string>(type: "text", nullable: false),
                    TwitchUsername = table.Column<string>(type: "text", nullable: false),
                    DiscordUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    AddedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BroadcasterTwitchChannelName = table.Column<string>(type: "text", nullable: false),
                    BroadcasterTwitchChannelId = table.Column<string>(type: "text", nullable: false),
                    BroadcasterTwitchChannelOAuthToken = table.Column<string>(type: "text", nullable: false),
                    BroadcasterTwitchChannelRefreshToken = table.Column<string>(type: "text", nullable: false),
                    BotTwitchChannelName = table.Column<string>(type: "text", nullable: false),
                    BotTwitchChannelId = table.Column<string>(type: "text", nullable: false),
                    BotTwitchChannelOAuthToken = table.Column<string>(type: "text", nullable: false),
                    BotTwitchChannelRefreshToken = table.Column<string>(type: "text", nullable: false),
                    DiscordGuildId = table.Column<string>(type: "text", nullable: true),
                    DiscordApiKey = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnvironmentalSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvironmentalSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChannelConfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    ChannelCurrencyName = table.Column<string>(type: "text", nullable: false),
                    CurrencyPointCap = table.Column<long>(type: "bigint", nullable: false),
                    StreamAnnounced = table.Column<bool>(type: "boolean", nullable: false),
                    StreamHappenedThisWeek = table.Column<bool>(type: "boolean", nullable: false),
                    DailyPointsCollectingAllowed = table.Column<string>(type: "text", nullable: false),
                    LastDailyPointsAllowed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubathonActive = table.Column<bool>(type: "boolean", nullable: false),
                    SubathonTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    DiscordGuildOwnerId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    DiscordEventChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    DiscordStreamAnnouncementChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    DiscordUserLinkingChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    DiscordUserCommandsChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    DiscordUserRankUpAnnouncementChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    DiscordGiveawayChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    DiscordReactionRoleChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    DiscordGeneralChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    DiscordBanRoleChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelConfig_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelConfig_ChannelId",
                table: "ChannelConfig",
                column: "ChannelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRefreshTokens_UserId",
                table: "UserRefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelConfig");

            migrationBuilder.DropTable(
                name: "ChannelUsers");

            migrationBuilder.DropTable(
                name: "EnvironmentalSettings");

            migrationBuilder.DropTable(
                name: "UserRefreshTokens");

            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
