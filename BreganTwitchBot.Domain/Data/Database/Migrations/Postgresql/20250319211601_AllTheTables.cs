using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BreganTwitchBot.Domain.Data.Database.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class AllTheTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanUseOpenAi",
                table: "ChannelUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "DailyPointsCollectingAllowed",
                table: "ChannelConfig",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "AiBookData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelUserId = table.Column<int>(type: "integer", nullable: false),
                    AiType = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiBookData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiBookData_ChannelUsers_ChannelUserId",
                        column: x => x.ChannelUserId,
                        principalTable: "ChannelUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Birthdays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelUserId = table.Column<int>(type: "integer", nullable: false),
                    Day = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Birthdays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Birthdays_ChannelUsers_ChannelUserId",
                        column: x => x.ChannelUserId,
                        principalTable: "ChannelUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Blacklist",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Word = table.Column<string>(type: "text", nullable: false),
                    WordType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blacklist", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChannelMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelMessages_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelRanks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    RankName = table.Column<string>(type: "text", nullable: false),
                    RankMinutesRequired = table.Column<int>(type: "integer", nullable: false),
                    BonusRankPointsEarned = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelRanks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelRanks_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelUserData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    ChannelUserId = table.Column<int>(type: "integer", nullable: false),
                    InStream = table.Column<bool>(type: "boolean", nullable: false),
                    IsSub = table.Column<bool>(type: "boolean", nullable: false),
                    IsSuperMod = table.Column<bool>(type: "boolean", nullable: false),
                    TimeoutStrikes = table.Column<int>(type: "integer", nullable: false),
                    WarnStrikes = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelUserData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelUserData_ChannelUsers_ChannelUserId",
                        column: x => x.ChannelUserId,
                        principalTable: "ChannelUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelUserData_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelUserGambleStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelUserId = table.Column<int>(type: "integer", nullable: false),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_ChannelUserGambleStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelUserGambleStats_ChannelUsers_ChannelUserId",
                        column: x => x.ChannelUserId,
                        principalTable: "ChannelUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelUserGambleStats_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelUserStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelUserId = table.Column<int>(type: "integer", nullable: false),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    TotalMessages = table.Column<int>(type: "integer", nullable: false),
                    GiftedSubsThisMonth = table.Column<int>(type: "integer", nullable: false),
                    BitsDonatedThisMonth = table.Column<int>(type: "integer", nullable: false),
                    MarblesWins = table.Column<int>(type: "integer", nullable: false),
                    BossesDone = table.Column<int>(type: "integer", nullable: false),
                    BossesPointsWon = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelUserStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelUserStats_ChannelUsers_ChannelUserId",
                        column: x => x.ChannelUserId,
                        principalTable: "ChannelUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelUserStats_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelUserWatchtime",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelUserId = table.Column<int>(type: "integer", nullable: false),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    MinutesInStream = table.Column<int>(type: "integer", nullable: false),
                    MinutesWatchedThisStream = table.Column<int>(type: "integer", nullable: false),
                    MinutesWatchedThisWeek = table.Column<int>(type: "integer", nullable: false),
                    MinutesWatchedThisMonth = table.Column<int>(type: "integer", nullable: false),
                    MinutesWatchedThisYear = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelUserWatchtime", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelUserWatchtime_ChannelUsers_ChannelUserId",
                        column: x => x.ChannelUserId,
                        principalTable: "ChannelUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelUserWatchtime_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomCommands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    CommandName = table.Column<string>(type: "text", nullable: false),
                    CommandText = table.Column<string>(type: "text", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimesUsed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomCommands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomCommands_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscordDailyPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelUserId = table.Column<int>(type: "integer", nullable: false),
                    DiscordDailyStreak = table.Column<int>(type: "integer", nullable: false),
                    DiscordDailyTotalClaims = table.Column<int>(type: "integer", nullable: false),
                    DiscordDailyClaimed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordDailyPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordDailyPoints_ChannelUsers_ChannelUserId",
                        column: x => x.ChannelUserId,
                        principalTable: "ChannelUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscordLinkRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    TwitchUserId = table.Column<string>(type: "text", nullable: false),
                    DiscordUserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordLinkRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordLinkRequests_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscordSpinStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_DiscordSpinStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordSpinStats_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscordUserStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelUserId = table.Column<int>(type: "integer", nullable: false),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    DiscordLevel = table.Column<int>(type: "integer", nullable: false),
                    DiscordXp = table.Column<int>(type: "integer", nullable: false),
                    DiscordLevelUpNotifsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    PrestigeLevel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUserStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordUserStats_ChannelUsers_ChannelUserId",
                        column: x => x.ChannelUserId,
                        principalTable: "ChannelUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscordUserStats_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StreamViewCounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamViewCounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StreamViewCounts_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subathons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    ChannelUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subathons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subathons_ChannelUsers_ChannelUserId",
                        column: x => x.ChannelUserId,
                        principalTable: "ChannelUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subathons_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TwitchDailyPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelUserId = table.Column<int>(type: "integer", nullable: false),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    CurrentDailyStreak = table.Column<int>(type: "integer", nullable: false),
                    HighestDailyStreak = table.Column<int>(type: "integer", nullable: false),
                    TotalTimesDailyClaimed = table.Column<int>(type: "integer", nullable: false),
                    TotalPointsClaimed = table.Column<long>(type: "bigint", nullable: false),
                    PointsLastClaimed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PointsClaimedThisStream = table.Column<bool>(type: "boolean", nullable: false),
                    WeeklyPointsClaimed = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentlyWeeklyStreak = table.Column<int>(type: "integer", nullable: false),
                    HighestWeeklyStreak = table.Column<int>(type: "integer", nullable: false),
                    TotalTimesWeeklyClaimed = table.Column<int>(type: "integer", nullable: false),
                    MonthlyPointsClaimed = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentMonthlyStreak = table.Column<int>(type: "integer", nullable: false),
                    HighestMonthlyStreak = table.Column<int>(type: "integer", nullable: false),
                    TotalTimesMonthlyClaimed = table.Column<int>(type: "integer", nullable: false),
                    YearlyPointsClaimed = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentYearlyStreak = table.Column<int>(type: "integer", nullable: false),
                    HighestYearlyStreak = table.Column<int>(type: "integer", nullable: false),
                    TotalTimesYearlyClaimed = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwitchDailyPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TwitchDailyPoints_ChannelUsers_ChannelUserId",
                        column: x => x.ChannelUserId,
                        principalTable: "ChannelUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TwitchDailyPoints_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TwitchSlotMachineStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    Tier1Wins = table.Column<int>(type: "integer", nullable: false),
                    Tier2Wins = table.Column<int>(type: "integer", nullable: false),
                    Tier3Wins = table.Column<int>(type: "integer", nullable: false),
                    JackpotWins = table.Column<int>(type: "integer", nullable: false),
                    TotalSpins = table.Column<int>(type: "integer", nullable: false),
                    SmorcWins = table.Column<int>(type: "integer", nullable: false),
                    JackpotAmount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwitchSlotMachineStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TwitchSlotMachineStats_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TwitchStreamStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    StreamId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_TwitchStreamStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TwitchStreamStats_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UniqueViewers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniqueViewers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniqueViewers_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelUserRankProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelUserId = table.Column<int>(type: "integer", nullable: false),
                    ChannelRankId = table.Column<int>(type: "integer", nullable: false),
                    AchievedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelUserRankProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelUserRankProgress_ChannelRanks_ChannelRankId",
                        column: x => x.ChannelRankId,
                        principalTable: "ChannelRanks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelUserRankProgress_ChannelUsers_ChannelUserId",
                        column: x => x.ChannelUserId,
                        principalTable: "ChannelUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiBookData_ChannelUserId",
                table: "AiBookData",
                column: "ChannelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Birthdays_ChannelUserId",
                table: "Birthdays",
                column: "ChannelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelMessages_ChannelId",
                table: "ChannelMessages",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelRanks_ChannelId",
                table: "ChannelRanks",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelUserData_ChannelId",
                table: "ChannelUserData",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelUserData_ChannelUserId",
                table: "ChannelUserData",
                column: "ChannelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelUserGambleStats_ChannelId",
                table: "ChannelUserGambleStats",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelUserGambleStats_ChannelUserId",
                table: "ChannelUserGambleStats",
                column: "ChannelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelUserRankProgress_ChannelRankId",
                table: "ChannelUserRankProgress",
                column: "ChannelRankId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelUserRankProgress_ChannelUserId",
                table: "ChannelUserRankProgress",
                column: "ChannelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelUserStats_ChannelId",
                table: "ChannelUserStats",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelUserStats_ChannelUserId",
                table: "ChannelUserStats",
                column: "ChannelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelUserWatchtime_ChannelId",
                table: "ChannelUserWatchtime",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelUserWatchtime_ChannelUserId",
                table: "ChannelUserWatchtime",
                column: "ChannelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomCommands_ChannelId",
                table: "CustomCommands",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordDailyPoints_ChannelUserId",
                table: "DiscordDailyPoints",
                column: "ChannelUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscordLinkRequests_ChannelId",
                table: "DiscordLinkRequests",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordSpinStats_ChannelId",
                table: "DiscordSpinStats",
                column: "ChannelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUserStats_ChannelId",
                table: "DiscordUserStats",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUserStats_ChannelUserId",
                table: "DiscordUserStats",
                column: "ChannelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StreamViewCounts_ChannelId",
                table: "StreamViewCounts",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Subathons_ChannelId",
                table: "Subathons",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Subathons_ChannelUserId",
                table: "Subathons",
                column: "ChannelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TwitchDailyPoints_ChannelId",
                table: "TwitchDailyPoints",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_TwitchDailyPoints_ChannelUserId",
                table: "TwitchDailyPoints",
                column: "ChannelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TwitchSlotMachineStats_ChannelId",
                table: "TwitchSlotMachineStats",
                column: "ChannelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TwitchStreamStats_ChannelId",
                table: "TwitchStreamStats",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_UniqueViewers_ChannelId",
                table: "UniqueViewers",
                column: "ChannelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiBookData");

            migrationBuilder.DropTable(
                name: "Birthdays");

            migrationBuilder.DropTable(
                name: "Blacklist");

            migrationBuilder.DropTable(
                name: "ChannelMessages");

            migrationBuilder.DropTable(
                name: "ChannelUserData");

            migrationBuilder.DropTable(
                name: "ChannelUserGambleStats");

            migrationBuilder.DropTable(
                name: "ChannelUserRankProgress");

            migrationBuilder.DropTable(
                name: "ChannelUserStats");

            migrationBuilder.DropTable(
                name: "ChannelUserWatchtime");

            migrationBuilder.DropTable(
                name: "CustomCommands");

            migrationBuilder.DropTable(
                name: "DiscordDailyPoints");

            migrationBuilder.DropTable(
                name: "DiscordLinkRequests");

            migrationBuilder.DropTable(
                name: "DiscordSpinStats");

            migrationBuilder.DropTable(
                name: "DiscordUserStats");

            migrationBuilder.DropTable(
                name: "StreamViewCounts");

            migrationBuilder.DropTable(
                name: "Subathons");

            migrationBuilder.DropTable(
                name: "TwitchDailyPoints");

            migrationBuilder.DropTable(
                name: "TwitchSlotMachineStats");

            migrationBuilder.DropTable(
                name: "TwitchStreamStats");

            migrationBuilder.DropTable(
                name: "UniqueViewers");

            migrationBuilder.DropTable(
                name: "ChannelRanks");

            migrationBuilder.DropColumn(
                name: "CanUseOpenAi",
                table: "ChannelUsers");

            migrationBuilder.AlterColumn<string>(
                name: "DailyPointsCollectingAllowed",
                table: "ChannelConfig",
                type: "text",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");
        }
    }
}
