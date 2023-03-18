using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "twitchbot_new");

            migrationBuilder.CreateTable(
                name: "Birthdays",
                schema: "twitchbot_new",
                columns: table => new
                {
                    DiscordId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Birthdays", x => x.DiscordId);
                });

            migrationBuilder.CreateTable(
                name: "Blacklist",
                schema: "twitchbot_new",
                columns: table => new
                {
                    Word = table.Column<string>(type: "text", nullable: false),
                    WordType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blacklist", x => x.Word);
                });

            migrationBuilder.CreateTable(
                name: "Commands",
                schema: "twitchbot_new",
                columns: table => new
                {
                    CommandName = table.Column<string>(type: "text", nullable: false),
                    CommandText = table.Column<string>(type: "text", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimesUsed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.CommandName);
                });

            migrationBuilder.CreateTable(
                name: "Config",
                schema: "twitchbot_new",
                columns: table => new
                {
                    BroadcasterName = table.Column<string>(type: "text", nullable: false),
                    BroadcasterOAuth = table.Column<string>(type: "text", nullable: false),
                    BroadcasterRefresh = table.Column<string>(type: "text", nullable: false),
                    StreamAnnounced = table.Column<bool>(type: "boolean", nullable: false),
                    DailyPointsCollectingAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    LastDailyPointsAllowed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubathonTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    PinnedStreamMessage = table.Column<string>(type: "text", nullable: false),
                    PinnedStreamMessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    PinnedStreamDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BotName = table.Column<string>(type: "text", nullable: false),
                    PointsName = table.Column<string>(type: "text", nullable: false),
                    TwitchChannelID = table.Column<string>(type: "text", nullable: false),
                    BotOAuth = table.Column<string>(type: "text", nullable: false),
                    TwitchAPIClientID = table.Column<string>(type: "text", nullable: false),
                    TwitchAPISecret = table.Column<string>(type: "text", nullable: false),
                    DiscordAPIKey = table.Column<string>(type: "text", nullable: false),
                    DiscordGuildOwner = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DiscordEventChannelID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DiscordStreamAnnouncementChannelID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DiscordLinkingChannelID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DiscordCommandsChannelID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DiscordRankUpAnnouncementChannelID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DiscordGiveawayChannelID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DiscordSocksChannelID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DiscordReactionRoleChannelID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DiscordGeneralChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DiscordGuildID = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DiscordBanRole = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    PrestigeCap = table.Column<long>(type: "bigint", nullable: false),
                    HFConnectionString = table.Column<string>(type: "text", nullable: false),
                    ProjectMonitorApiKey = table.Column<string>(type: "text", nullable: false),
                    TwitchBotApiKey = table.Column<string>(type: "text", nullable: false),
                    TwitchBotApiRefresh = table.Column<string>(type: "text", nullable: false),
                    BotChannelId = table.Column<string>(type: "text", nullable: false),
                    AiEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SubathonActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config", x => x.BroadcasterName);
                });

            migrationBuilder.CreateTable(
                name: "DiscordGiveaways",
                schema: "twitchbot_new",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GiveawayId = table.Column<string>(type: "text", nullable: false),
                    EligbleToWin = table.Column<bool>(type: "boolean", nullable: false),
                    DiscordUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordGiveaways", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscordLinkRequests",
                schema: "twitchbot_new",
                columns: table => new
                {
                    TwitchName = table.Column<string>(type: "text", nullable: false),
                    DiscordID = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordLinkRequests", x => x.TwitchName);
                });

            migrationBuilder.CreateTable(
                name: "RankBeggar",
                schema: "twitchbot_new",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Message = table.Column<string>(type: "text", nullable: false),
                    AiResult = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankBeggar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SlotMachine",
                schema: "twitchbot_new",
                columns: table => new
                {
                    StreamName = table.Column<string>(type: "text", nullable: false),
                    Tier1Wins = table.Column<int>(type: "integer", nullable: false),
                    Tier2Wins = table.Column<int>(type: "integer", nullable: false),
                    Tier3Wins = table.Column<int>(type: "integer", nullable: false),
                    JackpotWins = table.Column<int>(type: "integer", nullable: false),
                    TotalSpins = table.Column<int>(type: "integer", nullable: false),
                    SmorcWins = table.Column<int>(type: "integer", nullable: false),
                    JackpotAmount = table.Column<long>(type: "bigint", nullable: false),
                    GrapesWins = table.Column<int>(type: "integer", nullable: false),
                    PineappleWins = table.Column<int>(type: "integer", nullable: false),
                    CherriesWins = table.Column<int>(type: "integer", nullable: false),
                    CucumberWins = table.Column<int>(type: "integer", nullable: false),
                    EggplantWins = table.Column<int>(type: "integer", nullable: false),
                    CheeseWins = table.Column<int>(type: "integer", nullable: false),
                    DiscordTotalSpins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlotMachine", x => x.StreamName);
                });

            migrationBuilder.CreateTable(
                name: "StreamStats",
                schema: "twitchbot_new",
                columns: table => new
                {
                    StreamId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AvgViewCount = table.Column<double>(type: "double precision", nullable: false),
                    PeakViewerCount = table.Column<long>(type: "bigint", nullable: false),
                    BitsDonated = table.Column<long>(type: "bigint", nullable: false),
                    CommandsSent = table.Column<long>(type: "bigint", nullable: false),
                    DiscordRanksEarnt = table.Column<long>(type: "bigint", nullable: false),
                    StartingFollowerCount = table.Column<long>(type: "bigint", nullable: false),
                    EndingFollowerCount = table.Column<long>(type: "bigint", nullable: false),
                    StartingSubscriberCount = table.Column<long>(type: "bigint", nullable: false),
                    EndingSubscriberCount = table.Column<long>(type: "bigint", nullable: false),
                    MessagesReceived = table.Column<long>(type: "bigint", nullable: false),
                    NewFollowers = table.Column<long>(type: "bigint", nullable: false),
                    NewSubscribers = table.Column<long>(type: "bigint", nullable: false),
                    PointsGainedSubscribing = table.Column<long>(type: "bigint", nullable: false),
                    PointsGainedWatching = table.Column<long>(type: "bigint", nullable: false),
                    PointsGambled = table.Column<long>(type: "bigint", nullable: false),
                    PointsLost = table.Column<long>(type: "bigint", nullable: false),
                    PointsWon = table.Column<long>(type: "bigint", nullable: false),
                    SongRequestsBlacklisted = table.Column<long>(type: "bigint", nullable: false),
                    SongRequestsLiked = table.Column<long>(type: "bigint", nullable: false),
                    SongRequestsSent = table.Column<long>(type: "bigint", nullable: false),
                    TotalBans = table.Column<long>(type: "bigint", nullable: false),
                    TotalTimeouts = table.Column<long>(type: "bigint", nullable: false),
                    NewGiftedSubs = table.Column<long>(type: "bigint", nullable: false),
                    UniquePeople = table.Column<long>(type: "bigint", nullable: false),
                    Uptime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    StreamStarted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StreamEnded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GiftedPoints = table.Column<long>(type: "bigint", nullable: false),
                    TotalSpins = table.Column<long>(type: "bigint", nullable: false),
                    KappaWins = table.Column<long>(type: "bigint", nullable: false),
                    ForeheadWins = table.Column<long>(type: "bigint", nullable: false),
                    LulWins = table.Column<long>(type: "bigint", nullable: false),
                    SmorcWins = table.Column<long>(type: "bigint", nullable: false),
                    JackpotWins = table.Column<long>(type: "bigint", nullable: false),
                    TotalUsersClaimed = table.Column<long>(type: "bigint", nullable: false),
                    TotalPointsClaimed = table.Column<long>(type: "bigint", nullable: false),
                    AmountOfUsersReset = table.Column<long>(type: "bigint", nullable: false),
                    AmountOfRewardsRedeemed = table.Column<long>(type: "bigint", nullable: false),
                    RewardRedeemCost = table.Column<long>(type: "bigint", nullable: false),
                    AmountOfDiscordUsersJoined = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamStats", x => x.StreamId);
                });

            migrationBuilder.CreateTable(
                name: "StreamViewCount",
                schema: "twitchbot_new",
                columns: table => new
                {
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamViewCount", x => x.Time);
                });

            migrationBuilder.CreateTable(
                name: "Subathon",
                schema: "twitchbot_new",
                columns: table => new
                {
                    Username = table.Column<string>(type: "text", nullable: false),
                    SubsGifted = table.Column<int>(type: "integer", nullable: false),
                    BitsDonated = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subathon", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "UniqueViewers",
                schema: "twitchbot_new",
                columns: table => new
                {
                    Username = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniqueViewers", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "twitchbot_new",
                columns: table => new
                {
                    TwitchUserId = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    DiscordUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Points = table.Column<long>(type: "bigint", nullable: false),
                    TotalMessages = table.Column<int>(type: "integer", nullable: false),
                    InStream = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeenDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSub = table.Column<bool>(type: "boolean", nullable: false),
                    IsSuperMod = table.Column<bool>(type: "boolean", nullable: false),
                    GiftedSubsThisMonth = table.Column<int>(type: "integer", nullable: false),
                    BitsDonatedThisMonth = table.Column<int>(type: "integer", nullable: false),
                    MarblesWins = table.Column<int>(type: "integer", nullable: false),
                    BossesDone = table.Column<int>(type: "integer", nullable: false),
                    BossesPointsWon = table.Column<long>(type: "bigint", nullable: false),
                    TimeoutStrikes = table.Column<int>(type: "integer", nullable: false),
                    WarnStrikes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.TwitchUserId);
                });

            migrationBuilder.CreateTable(
                name: "DailyPoints",
                schema: "twitchbot_new",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TwitchUserId = table.Column<string>(type: "text", nullable: false),
                    CurrentStreak = table.Column<int>(type: "integer", nullable: false),
                    HighestStreak = table.Column<int>(type: "integer", nullable: false),
                    TotalTimesClaimed = table.Column<int>(type: "integer", nullable: false),
                    TotalPointsClaimed = table.Column<long>(type: "bigint", nullable: false),
                    PointsLastClaimed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PointsClaimedThisStream = table.Column<bool>(type: "boolean", nullable: false),
                    DiscordDailyStreak = table.Column<int>(type: "integer", nullable: false),
                    DiscordDailyTotalClaims = table.Column<int>(type: "integer", nullable: false),
                    DiscordDailyClaimed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyPoints_Users_TwitchUserId",
                        column: x => x.TwitchUserId,
                        principalSchema: "twitchbot_new",
                        principalTable: "Users",
                        principalColumn: "TwitchUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscordUserStats",
                schema: "twitchbot_new",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TwitchUserId = table.Column<string>(type: "text", nullable: false),
                    DiscordLevel = table.Column<int>(type: "integer", nullable: false),
                    DiscordXp = table.Column<int>(type: "integer", nullable: false),
                    DiscordLevelUpNotifsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PrestigeLevel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUserStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordUserStats_Users_TwitchUserId",
                        column: x => x.TwitchUserId,
                        principalSchema: "twitchbot_new",
                        principalTable: "Users",
                        principalColumn: "TwitchUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGambleStats",
                schema: "twitchbot_new",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TwitchUserId = table.Column<string>(type: "text", nullable: false),
                    PointsGambled = table.Column<long>(type: "bigint", nullable: false),
                    PointsWon = table.Column<long>(type: "bigint", nullable: false),
                    PointsLost = table.Column<long>(type: "bigint", nullable: false),
                    TotalSpins = table.Column<int>(type: "integer", nullable: false),
                    Tier1Wins = table.Column<int>(type: "integer", nullable: false),
                    Tier2Wins = table.Column<int>(type: "integer", nullable: false),
                    Tier3Wins = table.Column<int>(type: "integer", nullable: false),
                    JackpotWins = table.Column<int>(type: "integer", nullable: false),
                    SmorcWins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGambleStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGambleStats_Users_TwitchUserId",
                        column: x => x.TwitchUserId,
                        principalSchema: "twitchbot_new",
                        principalTable: "Users",
                        principalColumn: "TwitchUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Watchtime",
                schema: "twitchbot_new",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TwitchUserId = table.Column<string>(type: "text", nullable: false),
                    MinutesInStream = table.Column<int>(type: "integer", nullable: false),
                    MinutesWatchedThisStream = table.Column<int>(type: "integer", nullable: false),
                    MinutesWatchedThisWeek = table.Column<int>(type: "integer", nullable: false),
                    MinutesWatchedThisMonth = table.Column<int>(type: "integer", nullable: false),
                    Rank1Applied = table.Column<bool>(type: "boolean", nullable: false),
                    Rank2Applied = table.Column<bool>(type: "boolean", nullable: false),
                    Rank3Applied = table.Column<bool>(type: "boolean", nullable: false),
                    Rank4Applied = table.Column<bool>(type: "boolean", nullable: false),
                    Rank5Applied = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Watchtime", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Watchtime_Users_TwitchUserId",
                        column: x => x.TwitchUserId,
                        principalSchema: "twitchbot_new",
                        principalTable: "Users",
                        principalColumn: "TwitchUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "twitchbot_new",
                table: "Config",
                columns: new[] { "BroadcasterName", "AiEnabled", "BotChannelId", "BotName", "BotOAuth", "BroadcasterOAuth", "BroadcasterRefresh", "DailyPointsCollectingAllowed", "DiscordAPIKey", "DiscordBanRole", "DiscordCommandsChannelID", "DiscordEventChannelID", "DiscordGeneralChannel", "DiscordGiveawayChannelID", "DiscordGuildID", "DiscordGuildOwner", "DiscordLinkingChannelID", "DiscordRankUpAnnouncementChannelID", "DiscordReactionRoleChannelID", "DiscordSocksChannelID", "DiscordStreamAnnouncementChannelID", "HFConnectionString", "LastDailyPointsAllowed", "PinnedStreamDate", "PinnedStreamMessage", "PinnedStreamMessageId", "PointsName", "PrestigeCap", "ProjectMonitorApiKey", "StreamAnnounced", "SubathonActive", "SubathonTime", "TwitchAPIClientID", "TwitchAPISecret", "TwitchBotApiKey", "TwitchBotApiRefresh", "TwitchChannelID" },
                values: new object[] { "", false, "", "", "", "", "", false, "", 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, "", new DateTime(2023, 3, 17, 14, 3, 38, 354, DateTimeKind.Utc).AddTicks(7752), new DateTime(2023, 3, 17, 14, 3, 38, 354, DateTimeKind.Utc).AddTicks(7748), "", 0m, "", 0L, "", false, false, new TimeSpan(0, 0, 0, 0, 0), "", "", "", "", "" });

            migrationBuilder.InsertData(
                schema: "twitchbot_new",
                table: "SlotMachine",
                columns: new[] { "StreamName", "CheeseWins", "CherriesWins", "CucumberWins", "DiscordTotalSpins", "EggplantWins", "GrapesWins", "JackpotAmount", "JackpotWins", "PineappleWins", "SmorcWins", "Tier1Wins", "Tier2Wins", "Tier3Wins", "TotalSpins" },
                values: new object[] { "", 0, 0, 0, 0, 0, 0, 0L, 0, 0, 0, 0, 0, 0, 0 });

            migrationBuilder.CreateIndex(
                name: "IX_DailyPoints_TwitchUserId",
                schema: "twitchbot_new",
                table: "DailyPoints",
                column: "TwitchUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUserStats_TwitchUserId",
                schema: "twitchbot_new",
                table: "DiscordUserStats",
                column: "TwitchUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserGambleStats_TwitchUserId",
                schema: "twitchbot_new",
                table: "UserGambleStats",
                column: "TwitchUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Watchtime_TwitchUserId",
                schema: "twitchbot_new",
                table: "Watchtime",
                column: "TwitchUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Birthdays",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "Blacklist",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "Commands",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "Config",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "DailyPoints",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "DiscordGiveaways",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "DiscordLinkRequests",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "DiscordUserStats",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "RankBeggar",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "SlotMachine",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "StreamStats",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "StreamViewCount",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "Subathon",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "UniqueViewers",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "UserGambleStats",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "Watchtime",
                schema: "twitchbot_new");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "twitchbot_new");
        }
    }
}
