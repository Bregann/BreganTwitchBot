﻿// <auto-generated />
using System;
using BreganTwitchBot.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BreganTwitchbot.Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20220423084648_postgres")]
    partial class postgres
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BreganTwitchBot.Data.Models.Blacklist", b =>
                {
                    b.Property<string>("Word")
                        .HasColumnType("text")
                        .HasColumnName("Word");

                    b.Property<string>("WordType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("WordType");

                    b.HasKey("Word");

                    b.ToTable("Blacklist");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.Commands", b =>
                {
                    b.Property<string>("CommandName")
                        .HasColumnType("text")
                        .HasColumnName("commandName");

                    b.Property<string>("CommandText")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("commandText");

                    b.Property<DateTime>("LastUsed")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("lastUsed");

                    b.Property<int>("TimesUsed")
                        .HasColumnType("integer")
                        .HasColumnName("timesUsed");

                    b.HasKey("CommandName");

                    b.ToTable("Commands");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.Config", b =>
                {
                    b.Property<string>("BroadcasterOAuth")
                        .HasColumnType("text")
                        .HasColumnName("broadcasterOAuth");

                    b.Property<string>("BroadcasterRefresh")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("broadcasterRefresh");

                    b.Property<bool>("DailyPointsCollectingAllowed")
                        .HasColumnType("boolean")
                        .HasColumnName("dailyPointsCollectingAllowed");

                    b.Property<DateTime>("LastDailyPointsAllowed")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("lastDailyPointsAllowed");

                    b.Property<bool>("StreamAnnounced")
                        .HasColumnType("boolean")
                        .HasColumnName("streamAnnounced");

                    b.HasKey("BroadcasterOAuth");

                    b.ToTable("Config");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.DiscordLinkRequests", b =>
                {
                    b.Property<string>("TwitchName")
                        .HasColumnType("text")
                        .HasColumnName("TwitchName");

                    b.Property<decimal>("DiscordID")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("discordID");

                    b.HasKey("TwitchName");

                    b.ToTable("DiscordLinkRequests");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.SlotMachine", b =>
                {
                    b.Property<string>("StreamName")
                        .HasColumnType("text")
                        .HasColumnName("StreamName");

                    b.Property<int>("CheeseWins")
                        .HasColumnType("integer")
                        .HasColumnName("cheeseWins");

                    b.Property<int>("CherriesWins")
                        .HasColumnType("integer")
                        .HasColumnName("cherriesWins");

                    b.Property<int>("CucumberWins")
                        .HasColumnType("integer")
                        .HasColumnName("cucumberWins");

                    b.Property<int>("DiscordTotalSpins")
                        .HasColumnType("integer")
                        .HasColumnName("discordTotalSpins");

                    b.Property<int>("EggplantWins")
                        .HasColumnType("integer")
                        .HasColumnName("eggplantWins");

                    b.Property<int>("GrapesWins")
                        .HasColumnType("integer")
                        .HasColumnName("grapesWins");

                    b.Property<long>("JackpotAmount")
                        .HasColumnType("bigint")
                        .HasColumnName("jackpotAmount");

                    b.Property<int>("JackpotWins")
                        .HasColumnType("integer")
                        .HasColumnName("jackpotWins");

                    b.Property<int>("PineappleWins")
                        .HasColumnType("integer")
                        .HasColumnName("pineappleWins");

                    b.Property<int>("SmorcWins")
                        .HasColumnType("integer")
                        .HasColumnName("smorcWins");

                    b.Property<int>("Tier1Wins")
                        .HasColumnType("integer")
                        .HasColumnName("tier1Wins");

                    b.Property<int>("Tier2Wins")
                        .HasColumnType("integer")
                        .HasColumnName("tier2Wins");

                    b.Property<int>("Tier3Wins")
                        .HasColumnType("integer")
                        .HasColumnName("tier3Wins");

                    b.Property<int>("TotalSpins")
                        .HasColumnType("integer")
                        .HasColumnName("totalSpins");

                    b.HasKey("StreamName");

                    b.ToTable("SlotMachine");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.StreamStats", b =>
                {
                    b.Property<long>("StreamId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("streamId");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("StreamId"));

                    b.Property<long>("AmountOfDiscordUsersJoined")
                        .HasColumnType("bigint")
                        .HasColumnName("amountOfDiscordUsersJoined");

                    b.Property<long>("AmountOfRewardsRedeemed")
                        .HasColumnType("bigint")
                        .HasColumnName("amountOfRewardsRedeemed");

                    b.Property<long>("AmountOfUsersReset")
                        .HasColumnType("bigint")
                        .HasColumnName("amountOfUsersReset");

                    b.Property<double>("AvgViewCount")
                        .HasColumnType("double precision")
                        .HasColumnName("avgViewCount");

                    b.Property<long>("BitsDonated")
                        .HasColumnType("bigint")
                        .HasColumnName("bitsDonated");

                    b.Property<long>("ChannelPointsRewardCost")
                        .HasColumnType("bigint")
                        .HasColumnName("channelPointsRewardCost");

                    b.Property<long>("ChannelPointsRewardsRedeemed")
                        .HasColumnType("bigint")
                        .HasColumnName("channelPointsRewardsRedeemed");

                    b.Property<long>("CommandsSent")
                        .HasColumnType("bigint")
                        .HasColumnName("commandsSent");

                    b.Property<long>("DiscordRanksEarnt")
                        .HasColumnType("bigint")
                        .HasColumnName("discordRanksEarnt");

                    b.Property<long>("EndingFollowerCount")
                        .HasColumnType("bigint")
                        .HasColumnName("endingFollowerCount");

                    b.Property<long>("EndingSubscriberCount")
                        .HasColumnType("bigint")
                        .HasColumnName("endingSubscriberCount");

                    b.Property<long>("ForeheadWins")
                        .HasColumnType("bigint")
                        .HasColumnName("foreheadWins");

                    b.Property<long>("GiftedPoints")
                        .HasColumnType("bigint")
                        .HasColumnName("giftedPoints");

                    b.Property<long>("JackpotWins")
                        .HasColumnType("bigint")
                        .HasColumnName("jackpotWins");

                    b.Property<long>("KappaWins")
                        .HasColumnType("bigint")
                        .HasColumnName("kappaWins");

                    b.Property<long>("LulWins")
                        .HasColumnType("bigint")
                        .HasColumnName("lulWins");

                    b.Property<long>("MessagesReceived")
                        .HasColumnType("bigint")
                        .HasColumnName("messagesReceived");

                    b.Property<long>("NewFollowers")
                        .HasColumnType("bigint")
                        .HasColumnName("newFollowers");

                    b.Property<long>("NewGiftedSubs")
                        .HasColumnType("bigint")
                        .HasColumnName("newGiftedSubs");

                    b.Property<long>("NewSubscribers")
                        .HasColumnType("bigint")
                        .HasColumnName("newSubscribers");

                    b.Property<long>("PeakViewerCount")
                        .HasColumnType("bigint")
                        .HasColumnName("peakViewerCount");

                    b.Property<long>("PointsGainedSubscribing")
                        .HasColumnType("bigint")
                        .HasColumnName("pointsGainedSubscribing");

                    b.Property<long>("PointsGainedWatching")
                        .HasColumnType("bigint")
                        .HasColumnName("pointsGainedWatching");

                    b.Property<long>("PointsGambled")
                        .HasColumnType("bigint")
                        .HasColumnName("pointsGambled");

                    b.Property<long>("PointsLost")
                        .HasColumnType("bigint")
                        .HasColumnName("pointsLost");

                    b.Property<long>("PointsWon")
                        .HasColumnType("bigint")
                        .HasColumnName("pointsWon");

                    b.Property<long>("RewardRedeemCost")
                        .HasColumnType("bigint")
                        .HasColumnName("rewardRedeemCost");

                    b.Property<long>("SmorcWins")
                        .HasColumnType("bigint")
                        .HasColumnName("smorcWins");

                    b.Property<long>("SongRequestsBlacklisted")
                        .HasColumnType("bigint")
                        .HasColumnName("songRequestsBlacklisted");

                    b.Property<long>("SongRequestsLiked")
                        .HasColumnType("bigint")
                        .HasColumnName("songRequestsLiked");

                    b.Property<long>("SongRequestsSent")
                        .HasColumnType("bigint")
                        .HasColumnName("songRequestsSent");

                    b.Property<long>("StartingFollowerCount")
                        .HasColumnType("bigint")
                        .HasColumnName("startingFollowerCount");

                    b.Property<long>("StartingSubscriberCount")
                        .HasColumnType("bigint")
                        .HasColumnName("startingSubscriberCount");

                    b.Property<DateTime>("StreamEnded")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("streamEnded");

                    b.Property<DateTime>("StreamStarted")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("streamStarted");

                    b.Property<long>("TotalBans")
                        .HasColumnType("bigint")
                        .HasColumnName("totalBans");

                    b.Property<long>("TotalLostStreaks")
                        .HasColumnType("bigint")
                        .HasColumnName("totalLostStreaks");

                    b.Property<long>("TotalPointsClaimed")
                        .HasColumnType("bigint")
                        .HasColumnName("totalPointsClaimed");

                    b.Property<long>("TotalSpins")
                        .HasColumnType("bigint")
                        .HasColumnName("totalSpins");

                    b.Property<long>("TotalTimeouts")
                        .HasColumnType("bigint")
                        .HasColumnName("totalTimeouts");

                    b.Property<long>("TotalUsersClaimed")
                        .HasColumnType("bigint")
                        .HasColumnName("totalUsersClaimed");

                    b.Property<long>("UniquePeople")
                        .HasColumnType("bigint")
                        .HasColumnName("uniquePeople");

                    b.Property<TimeSpan>("Uptime")
                        .HasColumnType("interval")
                        .HasColumnName("uptime");

                    b.HasKey("StreamId");

                    b.ToTable("StreamStats");
                });

            modelBuilder.Entity("BreganTwitchbot.Data.Models.StreamViewCount", b =>
                {
                    b.Property<DateTime>("Time")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("Time");

                    b.Property<int>("ViewCount")
                        .HasColumnType("integer")
                        .HasColumnName("ViewCount");

                    b.HasKey("Time");

                    b.ToTable("StreamViewCount");
                });

            modelBuilder.Entity("BreganTwitchbot.Data.Models.UniqueViewers", b =>
                {
                    b.Property<string>("Username")
                        .HasColumnType("text")
                        .HasColumnName("Username");

                    b.HasKey("Username");

                    b.ToTable("UniqueViewers");
                });

            modelBuilder.Entity("BreganTwitchBot.Data.Models.Users", b =>
                {
                    b.Property<string>("TwitchUserId")
                        .HasColumnType("text")
                        .HasColumnName("twitchUserId");

                    b.Property<int>("BitsDonatedThisMonth")
                        .HasColumnType("integer")
                        .HasColumnName("bitsDonatedThisMonth");

                    b.Property<int>("BonusDiceRolls")
                        .HasColumnType("integer")
                        .HasColumnName("bonusDiceRolls");

                    b.Property<int>("BossesDone")
                        .HasColumnType("integer")
                        .HasColumnName("bossesDone");

                    b.Property<long>("BossesPointsWon")
                        .HasColumnType("bigint")
                        .HasColumnName("bossesPointsWon");

                    b.Property<int>("CurrentStreak")
                        .HasColumnType("integer")
                        .HasColumnName("currentStreak");

                    b.Property<int>("DiceRolls")
                        .HasColumnType("integer")
                        .HasColumnName("diceRolls");

                    b.Property<bool>("DiscordDailyClaimed")
                        .HasColumnType("boolean")
                        .HasColumnName("discordDailyClaimed");

                    b.Property<int>("DiscordDailyStreak")
                        .HasColumnType("integer")
                        .HasColumnName("discordDailyStreak");

                    b.Property<int>("DiscordDailyTotalClaims")
                        .HasColumnType("integer")
                        .HasColumnName("discordDailyTotalClaims");

                    b.Property<int>("DiscordLevel")
                        .HasColumnType("integer")
                        .HasColumnName("discordLevel");

                    b.Property<bool>("DiscordLevelUpNotifsEnabled")
                        .HasColumnType("boolean")
                        .HasColumnName("discordLevelUpNotifsEnabled");

                    b.Property<decimal>("DiscordUserId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("discordUserId");

                    b.Property<int>("DiscordXp")
                        .HasColumnType("integer")
                        .HasColumnName("discordXp");

                    b.Property<int>("GiftedSubsThisMonth")
                        .HasColumnType("integer")
                        .HasColumnName("giftedSubsThisMonth");

                    b.Property<int>("HighestStreak")
                        .HasColumnType("integer")
                        .HasColumnName("highestStreak");

                    b.Property<bool>("InStream")
                        .HasColumnType("boolean")
                        .HasColumnName("inStream");

                    b.Property<bool>("IsSub")
                        .HasColumnType("boolean")
                        .HasColumnName("isSub");

                    b.Property<bool>("IsSuperMod")
                        .HasColumnType("boolean")
                        .HasColumnName("isSuperMod");

                    b.Property<int>("JackpotWins")
                        .HasColumnType("integer")
                        .HasColumnName("jackpotWins");

                    b.Property<DateTime>("LastSeenDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("lastSeenDate");

                    b.Property<int>("MarblesWins")
                        .HasColumnType("integer")
                        .HasColumnName("marblesWins");

                    b.Property<int>("MinutesInStream")
                        .HasColumnType("integer")
                        .HasColumnName("minutesInStream");

                    b.Property<int>("MinutesWatchedThisMonth")
                        .HasColumnType("integer")
                        .HasColumnName("minutesWatchedThisMonth");

                    b.Property<int>("MinutesWatchedThisStream")
                        .HasColumnType("integer")
                        .HasColumnName("minutesWatchedThisStream");

                    b.Property<int>("MinutesWatchedThisWeek")
                        .HasColumnType("integer")
                        .HasColumnName("minutesWatchedThisWeek");

                    b.Property<long>("Points")
                        .HasColumnType("bigint")
                        .HasColumnName("points");

                    b.Property<bool>("PointsClaimedThisStream")
                        .HasColumnType("boolean")
                        .HasColumnName("pointsClaimedThisStream");

                    b.Property<long>("PointsGambled")
                        .HasColumnType("bigint")
                        .HasColumnName("pointsGambled");

                    b.Property<DateTime>("PointsLastClaimed")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("pointsLastClaimed");

                    b.Property<long>("PointsLost")
                        .HasColumnType("bigint")
                        .HasColumnName("pointsLost");

                    b.Property<long>("PointsWon")
                        .HasColumnType("bigint")
                        .HasColumnName("pointsWon");

                    b.Property<int>("PrestigeLevel")
                        .HasColumnType("integer")
                        .HasColumnName("prestigeLevel");

                    b.Property<bool>("Rank1Applied")
                        .HasColumnType("boolean")
                        .HasColumnName("rank1Applied");

                    b.Property<bool>("Rank2Applied")
                        .HasColumnType("boolean")
                        .HasColumnName("rank2Applied");

                    b.Property<bool>("Rank3Applied")
                        .HasColumnType("boolean")
                        .HasColumnName("rank3Applied");

                    b.Property<bool>("Rank4Applied")
                        .HasColumnType("boolean")
                        .HasColumnName("rank4Applied");

                    b.Property<bool>("Rank5Applied")
                        .HasColumnType("boolean")
                        .HasColumnName("rank5Applied");

                    b.Property<int>("SmorcWins")
                        .HasColumnType("integer")
                        .HasColumnName("smorcWins");

                    b.Property<int>("Tier1Wins")
                        .HasColumnType("integer")
                        .HasColumnName("tier1Wins");

                    b.Property<int>("Tier2Wins")
                        .HasColumnType("integer")
                        .HasColumnName("tier2Wins");

                    b.Property<int>("Tier3Wins")
                        .HasColumnType("integer")
                        .HasColumnName("tier3Wins");

                    b.Property<int>("TimeoutStrikes")
                        .HasColumnType("integer")
                        .HasColumnName("timeoutStrikes");

                    b.Property<int>("TotalMessages")
                        .HasColumnType("integer")
                        .HasColumnName("totalMessages");

                    b.Property<long>("TotalPointsClaimed")
                        .HasColumnType("bigint")
                        .HasColumnName("totalPointsClaimed");

                    b.Property<int>("TotalSpins")
                        .HasColumnType("integer")
                        .HasColumnName("totalSpins");

                    b.Property<int>("TotalTimesClaimed")
                        .HasColumnType("integer")
                        .HasColumnName("totalTimesClaimed");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.Property<int>("WarnStrikes")
                        .HasColumnType("integer")
                        .HasColumnName("warnStrikes");

                    b.HasKey("TwitchUserId");

                    b.HasIndex("Points");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
