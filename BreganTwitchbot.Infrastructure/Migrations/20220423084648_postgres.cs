using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    public partial class postgres : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blacklist",
                columns: table => new
                {
                    Word = table.Column<string>(type: "text", nullable: false),
                    WordType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blacklist", x => x.Word);
                });

            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    commandName = table.Column<string>(type: "text", nullable: false),
                    commandText = table.Column<string>(type: "text", nullable: false),
                    lastUsed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    timesUsed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.commandName);
                });

            migrationBuilder.CreateTable(
                name: "Config",
                columns: table => new
                {
                    broadcasterOAuth = table.Column<string>(type: "text", nullable: false),
                    broadcasterRefresh = table.Column<string>(type: "text", nullable: false),
                    streamAnnounced = table.Column<bool>(type: "boolean", nullable: false),
                    dailyPointsCollectingAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    lastDailyPointsAllowed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config", x => x.broadcasterOAuth);
                });

            migrationBuilder.CreateTable(
                name: "DiscordLinkRequests",
                columns: table => new
                {
                    TwitchName = table.Column<string>(type: "text", nullable: false),
                    discordID = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordLinkRequests", x => x.TwitchName);
                });

            migrationBuilder.CreateTable(
                name: "SlotMachine",
                columns: table => new
                {
                    StreamName = table.Column<string>(type: "text", nullable: false),
                    tier1Wins = table.Column<int>(type: "integer", nullable: false),
                    tier2Wins = table.Column<int>(type: "integer", nullable: false),
                    tier3Wins = table.Column<int>(type: "integer", nullable: false),
                    jackpotWins = table.Column<int>(type: "integer", nullable: false),
                    totalSpins = table.Column<int>(type: "integer", nullable: false),
                    smorcWins = table.Column<int>(type: "integer", nullable: false),
                    jackpotAmount = table.Column<long>(type: "bigint", nullable: false),
                    grapesWins = table.Column<int>(type: "integer", nullable: false),
                    pineappleWins = table.Column<int>(type: "integer", nullable: false),
                    cherriesWins = table.Column<int>(type: "integer", nullable: false),
                    cucumberWins = table.Column<int>(type: "integer", nullable: false),
                    eggplantWins = table.Column<int>(type: "integer", nullable: false),
                    cheeseWins = table.Column<int>(type: "integer", nullable: false),
                    discordTotalSpins = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlotMachine", x => x.StreamName);
                });

            migrationBuilder.CreateTable(
                name: "StreamStats",
                columns: table => new
                {
                    streamId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    avgViewCount = table.Column<double>(type: "double precision", nullable: false),
                    peakViewerCount = table.Column<long>(type: "bigint", nullable: false),
                    bitsDonated = table.Column<long>(type: "bigint", nullable: false),
                    commandsSent = table.Column<long>(type: "bigint", nullable: false),
                    discordRanksEarnt = table.Column<long>(type: "bigint", nullable: false),
                    startingFollowerCount = table.Column<long>(type: "bigint", nullable: false),
                    endingFollowerCount = table.Column<long>(type: "bigint", nullable: false),
                    startingSubscriberCount = table.Column<long>(type: "bigint", nullable: false),
                    endingSubscriberCount = table.Column<long>(type: "bigint", nullable: false),
                    messagesReceived = table.Column<long>(type: "bigint", nullable: false),
                    newFollowers = table.Column<long>(type: "bigint", nullable: false),
                    newSubscribers = table.Column<long>(type: "bigint", nullable: false),
                    pointsGainedSubscribing = table.Column<long>(type: "bigint", nullable: false),
                    pointsGainedWatching = table.Column<long>(type: "bigint", nullable: false),
                    pointsGambled = table.Column<long>(type: "bigint", nullable: false),
                    pointsLost = table.Column<long>(type: "bigint", nullable: false),
                    pointsWon = table.Column<long>(type: "bigint", nullable: false),
                    songRequestsBlacklisted = table.Column<long>(type: "bigint", nullable: false),
                    songRequestsLiked = table.Column<long>(type: "bigint", nullable: false),
                    songRequestsSent = table.Column<long>(type: "bigint", nullable: false),
                    totalBans = table.Column<long>(type: "bigint", nullable: false),
                    totalTimeouts = table.Column<long>(type: "bigint", nullable: false),
                    newGiftedSubs = table.Column<long>(type: "bigint", nullable: false),
                    uniquePeople = table.Column<long>(type: "bigint", nullable: false),
                    uptime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    streamStarted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    streamEnded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    giftedPoints = table.Column<long>(type: "bigint", nullable: false),
                    totalSpins = table.Column<long>(type: "bigint", nullable: false),
                    kappaWins = table.Column<long>(type: "bigint", nullable: false),
                    foreheadWins = table.Column<long>(type: "bigint", nullable: false),
                    lulWins = table.Column<long>(type: "bigint", nullable: false),
                    smorcWins = table.Column<long>(type: "bigint", nullable: false),
                    jackpotWins = table.Column<long>(type: "bigint", nullable: false),
                    totalUsersClaimed = table.Column<long>(type: "bigint", nullable: false),
                    totalPointsClaimed = table.Column<long>(type: "bigint", nullable: false),
                    amountOfUsersReset = table.Column<long>(type: "bigint", nullable: false),
                    amountOfRewardsRedeemed = table.Column<long>(type: "bigint", nullable: false),
                    rewardRedeemCost = table.Column<long>(type: "bigint", nullable: false),
                    amountOfDiscordUsersJoined = table.Column<long>(type: "bigint", nullable: false),
                    channelPointsRewardsRedeemed = table.Column<long>(type: "bigint", nullable: false),
                    channelPointsRewardCost = table.Column<long>(type: "bigint", nullable: false),
                    totalLostStreaks = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamStats", x => x.streamId);
                });

            migrationBuilder.CreateTable(
                name: "StreamViewCount",
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
                name: "UniqueViewers",
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
                columns: table => new
                {
                    twitchUserId = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    inStream = table.Column<bool>(type: "boolean", nullable: false),
                    isSub = table.Column<bool>(type: "boolean", nullable: false),
                    minutesInStream = table.Column<int>(type: "integer", nullable: false),
                    points = table.Column<long>(type: "bigint", nullable: false),
                    isSuperMod = table.Column<bool>(type: "boolean", nullable: false),
                    totalMessages = table.Column<int>(type: "integer", nullable: false),
                    discordUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    lastSeenDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    pointsGambled = table.Column<long>(type: "bigint", nullable: false),
                    pointsWon = table.Column<long>(type: "bigint", nullable: false),
                    pointsLost = table.Column<long>(type: "bigint", nullable: false),
                    totalSpins = table.Column<int>(type: "integer", nullable: false),
                    tier1Wins = table.Column<int>(type: "integer", nullable: false),
                    tier2Wins = table.Column<int>(type: "integer", nullable: false),
                    tier3Wins = table.Column<int>(type: "integer", nullable: false),
                    jackpotWins = table.Column<int>(type: "integer", nullable: false),
                    smorcWins = table.Column<int>(type: "integer", nullable: false),
                    currentStreak = table.Column<int>(type: "integer", nullable: false),
                    highestStreak = table.Column<int>(type: "integer", nullable: false),
                    totalTimesClaimed = table.Column<int>(type: "integer", nullable: false),
                    totalPointsClaimed = table.Column<long>(type: "bigint", nullable: false),
                    pointsLastClaimed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    pointsClaimedThisStream = table.Column<bool>(type: "boolean", nullable: false),
                    giftedSubsThisMonth = table.Column<int>(type: "integer", nullable: false),
                    bitsDonatedThisMonth = table.Column<int>(type: "integer", nullable: false),
                    marblesWins = table.Column<int>(type: "integer", nullable: false),
                    diceRolls = table.Column<int>(type: "integer", nullable: false),
                    bonusDiceRolls = table.Column<int>(type: "integer", nullable: false),
                    discordDailyStreak = table.Column<int>(type: "integer", nullable: false),
                    discordDailyTotalClaims = table.Column<int>(type: "integer", nullable: false),
                    discordDailyClaimed = table.Column<bool>(type: "boolean", nullable: false),
                    discordLevel = table.Column<int>(type: "integer", nullable: false),
                    discordXp = table.Column<int>(type: "integer", nullable: false),
                    discordLevelUpNotifsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    prestigeLevel = table.Column<int>(type: "integer", nullable: false),
                    minutesWatchedThisStream = table.Column<int>(type: "integer", nullable: false),
                    minutesWatchedThisWeek = table.Column<int>(type: "integer", nullable: false),
                    minutesWatchedThisMonth = table.Column<int>(type: "integer", nullable: false),
                    bossesDone = table.Column<int>(type: "integer", nullable: false),
                    bossesPointsWon = table.Column<long>(type: "bigint", nullable: false),
                    timeoutStrikes = table.Column<int>(type: "integer", nullable: false),
                    warnStrikes = table.Column<int>(type: "integer", nullable: false),
                    rank1Applied = table.Column<bool>(type: "boolean", nullable: false),
                    rank2Applied = table.Column<bool>(type: "boolean", nullable: false),
                    rank3Applied = table.Column<bool>(type: "boolean", nullable: false),
                    rank4Applied = table.Column<bool>(type: "boolean", nullable: false),
                    rank5Applied = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.twitchUserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_points",
                table: "Users",
                column: "points");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blacklist");

            migrationBuilder.DropTable(
                name: "Commands");

            migrationBuilder.DropTable(
                name: "Config");

            migrationBuilder.DropTable(
                name: "DiscordLinkRequests");

            migrationBuilder.DropTable(
                name: "SlotMachine");

            migrationBuilder.DropTable(
                name: "StreamStats");

            migrationBuilder.DropTable(
                name: "StreamViewCount");

            migrationBuilder.DropTable(
                name: "UniqueViewers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}