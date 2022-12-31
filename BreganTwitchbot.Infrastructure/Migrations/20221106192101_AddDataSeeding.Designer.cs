﻿// <auto-generated />
using System;
using BreganTwitchBot.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20221106192101_AddDataSeeding")]
    partial class AddDataSeeding
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BreganTwitchBot.Data.Models.Birthdays", b =>
                {
                    b.Property<decimal>("DiscordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Day")
                        .HasColumnType("integer");

                    b.Property<int>("Month")
                        .HasColumnType("integer");

                    b.HasKey("DiscordId");

                    b.ToTable("Birthdays");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.Blacklist", b =>
                {
                    b.Property<string>("Word")
                        .HasColumnType("text");

                    b.Property<string>("WordType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Word");

                    b.ToTable("Blacklist");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.Commands", b =>
                {
                    b.Property<string>("CommandName")
                        .HasColumnType("text");

                    b.Property<string>("CommandText")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("LastUsed")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("TimesUsed")
                        .HasColumnType("integer");

                    b.HasKey("CommandName");

                    b.ToTable("Commands");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.Config", b =>
                {
                    b.Property<string>("BroadcasterName")
                        .HasColumnType("text");

                    b.Property<string>("BotName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BotOAuth")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BroadcasterOAuth")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BroadcasterRefresh")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("DailyPointsCollectingAllowed")
                        .HasColumnType("boolean");

                    b.Property<string>("DiscordAPIKey")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("DiscordBanRole")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("DiscordCommandsChannelID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("DiscordEventChannelID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("DiscordGeneralChannel")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("DiscordGiveawayChannelID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("DiscordGuildID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("DiscordGuildOwner")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("DiscordLinkingChannelID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("DiscordRankUpAnnouncementChannelID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("DiscordReactionRoleChannelID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("DiscordSocksChannelID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("DiscordStreamAnnouncementChannelID")
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("LastDailyPointsAllowed")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("PinnedStreamDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PinnedStreamMessage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("PinnedStreamMessageId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("PointsName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("PrestigeCap")
                        .HasColumnType("bigint");

                    b.Property<bool>("StreamAnnounced")
                        .HasColumnType("boolean");

                    b.Property<TimeSpan>("SubathonTime")
                        .HasColumnType("interval");

                    b.Property<string>("TwitchAPIClientID")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TwitchAPISecret")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TwitchChannelID")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("BroadcasterName");

                    b.ToTable("Config");

                    b.HasData(
                        new
                        {
                            BroadcasterName = "",
                            BotName = "",
                            BotOAuth = "",
                            BroadcasterOAuth = "",
                            BroadcasterRefresh = "",
                            DailyPointsCollectingAllowed = false,
                            DiscordAPIKey = "",
                            DiscordBanRole = 0m,
                            DiscordCommandsChannelID = 0m,
                            DiscordEventChannelID = 0m,
                            DiscordGeneralChannel = 0m,
                            DiscordGiveawayChannelID = 0m,
                            DiscordGuildID = 0m,
                            DiscordGuildOwner = 0m,
                            DiscordLinkingChannelID = 0m,
                            DiscordRankUpAnnouncementChannelID = 0m,
                            DiscordReactionRoleChannelID = 0m,
                            DiscordSocksChannelID = 0m,
                            DiscordStreamAnnouncementChannelID = 0m,
                            LastDailyPointsAllowed = new DateTime(2022, 11, 5, 19, 21, 1, 156, DateTimeKind.Utc).AddTicks(8214),
                            PinnedStreamDate = new DateTime(2022, 11, 5, 19, 21, 1, 156, DateTimeKind.Utc).AddTicks(8200),
                            PinnedStreamMessage = "",
                            PinnedStreamMessageId = 0m,
                            PointsName = "",
                            PrestigeCap = 0L,
                            StreamAnnounced = false,
                            SubathonTime = new TimeSpan(0, 0, 0, 0, 0),
                            TwitchAPIClientID = "",
                            TwitchAPISecret = "",
                            TwitchChannelID = ""
                        });
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.DiscordLinkRequests", b =>
                {
                    b.Property<string>("TwitchName")
                        .HasColumnType("text");

                    b.Property<decimal>("DiscordID")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("TwitchName");

                    b.ToTable("DiscordLinkRequests");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.RankBeggar", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AiResult")
                        .HasColumnType("integer");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("RankBeggar");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.SlotMachine", b =>
                {
                    b.Property<string>("StreamName")
                        .HasColumnType("text");

                    b.Property<int>("CheeseWins")
                        .HasColumnType("integer");

                    b.Property<int>("CherriesWins")
                        .HasColumnType("integer");

                    b.Property<int>("CucumberWins")
                        .HasColumnType("integer");

                    b.Property<int>("DiscordTotalSpins")
                        .HasColumnType("integer");

                    b.Property<int>("EggplantWins")
                        .HasColumnType("integer");

                    b.Property<int>("GrapesWins")
                        .HasColumnType("integer");

                    b.Property<long>("JackpotAmount")
                        .HasColumnType("bigint");

                    b.Property<int>("JackpotWins")
                        .HasColumnType("integer");

                    b.Property<int>("PineappleWins")
                        .HasColumnType("integer");

                    b.Property<int>("SmorcWins")
                        .HasColumnType("integer");

                    b.Property<int>("Tier1Wins")
                        .HasColumnType("integer");

                    b.Property<int>("Tier2Wins")
                        .HasColumnType("integer");

                    b.Property<int>("Tier3Wins")
                        .HasColumnType("integer");

                    b.Property<int>("TotalSpins")
                        .HasColumnType("integer");

                    b.HasKey("StreamName");

                    b.ToTable("SlotMachine");

                    b.HasData(
                        new
                        {
                            StreamName = "",
                            CheeseWins = 0,
                            CherriesWins = 0,
                            CucumberWins = 0,
                            DiscordTotalSpins = 0,
                            EggplantWins = 0,
                            GrapesWins = 0,
                            JackpotAmount = 0L,
                            JackpotWins = 0,
                            PineappleWins = 0,
                            SmorcWins = 0,
                            Tier1Wins = 0,
                            Tier2Wins = 0,
                            Tier3Wins = 0,
                            TotalSpins = 0
                        });
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.StreamStats", b =>
                {
                    b.Property<long>("StreamId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("StreamId"));

                    b.Property<long>("AmountOfDiscordUsersJoined")
                        .HasColumnType("bigint");

                    b.Property<long>("AmountOfRewardsRedeemed")
                        .HasColumnType("bigint");

                    b.Property<long>("AmountOfUsersReset")
                        .HasColumnType("bigint");

                    b.Property<double>("AvgViewCount")
                        .HasColumnType("double precision");

                    b.Property<long>("BitsDonated")
                        .HasColumnType("bigint");

                    b.Property<long>("CommandsSent")
                        .HasColumnType("bigint");

                    b.Property<long>("DiscordRanksEarnt")
                        .HasColumnType("bigint");

                    b.Property<long>("EndingFollowerCount")
                        .HasColumnType("bigint");

                    b.Property<long>("EndingSubscriberCount")
                        .HasColumnType("bigint");

                    b.Property<long>("ForeheadWins")
                        .HasColumnType("bigint");

                    b.Property<long>("GiftedPoints")
                        .HasColumnType("bigint");

                    b.Property<long>("JackpotWins")
                        .HasColumnType("bigint");

                    b.Property<long>("KappaWins")
                        .HasColumnType("bigint");

                    b.Property<long>("LulWins")
                        .HasColumnType("bigint");

                    b.Property<long>("MessagesReceived")
                        .HasColumnType("bigint");

                    b.Property<long>("NewFollowers")
                        .HasColumnType("bigint");

                    b.Property<long>("NewGiftedSubs")
                        .HasColumnType("bigint");

                    b.Property<long>("NewSubscribers")
                        .HasColumnType("bigint");

                    b.Property<long>("PeakViewerCount")
                        .HasColumnType("bigint");

                    b.Property<long>("PointsGainedSubscribing")
                        .HasColumnType("bigint");

                    b.Property<long>("PointsGainedWatching")
                        .HasColumnType("bigint");

                    b.Property<long>("PointsGambled")
                        .HasColumnType("bigint");

                    b.Property<long>("PointsLost")
                        .HasColumnType("bigint");

                    b.Property<long>("PointsWon")
                        .HasColumnType("bigint");

                    b.Property<long>("RewardRedeemCost")
                        .HasColumnType("bigint");

                    b.Property<long>("SmorcWins")
                        .HasColumnType("bigint");

                    b.Property<long>("SongRequestsBlacklisted")
                        .HasColumnType("bigint");

                    b.Property<long>("SongRequestsLiked")
                        .HasColumnType("bigint");

                    b.Property<long>("SongRequestsSent")
                        .HasColumnType("bigint");

                    b.Property<long>("StartingFollowerCount")
                        .HasColumnType("bigint");

                    b.Property<long>("StartingSubscriberCount")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("StreamEnded")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("StreamStarted")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("TotalBans")
                        .HasColumnType("bigint");

                    b.Property<long>("TotalPointsClaimed")
                        .HasColumnType("bigint");

                    b.Property<long>("TotalSpins")
                        .HasColumnType("bigint");

                    b.Property<long>("TotalTimeouts")
                        .HasColumnType("bigint");

                    b.Property<long>("TotalUsersClaimed")
                        .HasColumnType("bigint");

                    b.Property<long>("UniquePeople")
                        .HasColumnType("bigint");

                    b.Property<TimeSpan>("Uptime")
                        .HasColumnType("interval");

                    b.HasKey("StreamId");

                    b.ToTable("StreamStats");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.StreamViewCount", b =>
                {
                    b.Property<DateTime>("Time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("ViewCount")
                        .HasColumnType("integer");

                    b.HasKey("Time");

                    b.ToTable("StreamViewCount");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.Subathon", b =>
                {
                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.Property<int>("BitsDonated")
                        .HasColumnType("integer");

                    b.Property<int>("SubsGifted")
                        .HasColumnType("integer");

                    b.HasKey("Username");

                    b.ToTable("Subathon");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.UniqueViewers", b =>
                {
                    b.Property<string>("Username")
                        .HasColumnType("text");

                    b.HasKey("Username");

                    b.ToTable("UniqueViewers");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.Users", b =>
                {
                    b.Property<string>("TwitchUserId")
                        .HasColumnType("text");

                    b.Property<int>("BitsDonatedThisMonth")
                        .HasColumnType("integer");

                    b.Property<int>("BonusDiceRolls")
                        .HasColumnType("integer");

                    b.Property<int>("BossesDone")
                        .HasColumnType("integer");

                    b.Property<long>("BossesPointsWon")
                        .HasColumnType("bigint");

                    b.Property<int>("CurrentStreak")
                        .HasColumnType("integer");

                    b.Property<int>("DiceRolls")
                        .HasColumnType("integer");

                    b.Property<bool>("DiscordDailyClaimed")
                        .HasColumnType("boolean");

                    b.Property<int>("DiscordDailyStreak")
                        .HasColumnType("integer");

                    b.Property<int>("DiscordDailyTotalClaims")
                        .HasColumnType("integer");

                    b.Property<int>("DiscordLevel")
                        .HasColumnType("integer");

                    b.Property<bool>("DiscordLevelUpNotifsEnabled")
                        .HasColumnType("boolean");

                    b.Property<decimal>("DiscordUserId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("DiscordXp")
                        .HasColumnType("integer");

                    b.Property<int>("GiftedSubsThisMonth")
                        .HasColumnType("integer");

                    b.Property<int>("HighestStreak")
                        .HasColumnType("integer");

                    b.Property<bool>("InStream")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsSub")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsSuperMod")
                        .HasColumnType("boolean");

                    b.Property<int>("JackpotWins")
                        .HasColumnType("integer");

                    b.Property<DateTime>("LastSeenDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("MarblesWins")
                        .HasColumnType("integer");

                    b.Property<int>("MinutesInStream")
                        .HasColumnType("integer");

                    b.Property<int>("MinutesWatchedThisMonth")
                        .HasColumnType("integer");

                    b.Property<int>("MinutesWatchedThisStream")
                        .HasColumnType("integer");

                    b.Property<int>("MinutesWatchedThisWeek")
                        .HasColumnType("integer");

                    b.Property<long>("Points")
                        .HasColumnType("bigint");

                    b.Property<bool>("PointsClaimedThisStream")
                        .HasColumnType("boolean");

                    b.Property<long>("PointsGambled")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("PointsLastClaimed")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("PointsLost")
                        .HasColumnType("bigint");

                    b.Property<long>("PointsWon")
                        .HasColumnType("bigint");

                    b.Property<int>("PrestigeLevel")
                        .HasColumnType("integer");

                    b.Property<bool>("Rank1Applied")
                        .HasColumnType("boolean");

                    b.Property<bool>("Rank2Applied")
                        .HasColumnType("boolean");

                    b.Property<bool>("Rank3Applied")
                        .HasColumnType("boolean");

                    b.Property<bool>("Rank4Applied")
                        .HasColumnType("boolean");

                    b.Property<bool>("Rank5Applied")
                        .HasColumnType("boolean");

                    b.Property<int>("SmorcWins")
                        .HasColumnType("integer");

                    b.Property<int>("Tier1Wins")
                        .HasColumnType("integer");

                    b.Property<int>("Tier2Wins")
                        .HasColumnType("integer");

                    b.Property<int>("Tier3Wins")
                        .HasColumnType("integer");

                    b.Property<int>("TimeoutStrikes")
                        .HasColumnType("integer");

                    b.Property<int>("TotalMessages")
                        .HasColumnType("integer");

                    b.Property<long>("TotalPointsClaimed")
                        .HasColumnType("bigint");

                    b.Property<int>("TotalSpins")
                        .HasColumnType("integer");

                    b.Property<int>("TotalTimesClaimed")
                        .HasColumnType("integer");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("WarnStrikes")
                        .HasColumnType("integer");

                    b.HasKey("TwitchUserId");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
