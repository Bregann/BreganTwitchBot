using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    public partial class Config : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "warnStrikes",
                table: "Users",
                newName: "WarnStrikes");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "totalTimesClaimed",
                table: "Users",
                newName: "TotalTimesClaimed");

            migrationBuilder.RenameColumn(
                name: "totalSpins",
                table: "Users",
                newName: "TotalSpins");

            migrationBuilder.RenameColumn(
                name: "totalPointsClaimed",
                table: "Users",
                newName: "TotalPointsClaimed");

            migrationBuilder.RenameColumn(
                name: "totalMessages",
                table: "Users",
                newName: "TotalMessages");

            migrationBuilder.RenameColumn(
                name: "timeoutStrikes",
                table: "Users",
                newName: "TimeoutStrikes");

            migrationBuilder.RenameColumn(
                name: "tier3Wins",
                table: "Users",
                newName: "Tier3Wins");

            migrationBuilder.RenameColumn(
                name: "tier2Wins",
                table: "Users",
                newName: "Tier2Wins");

            migrationBuilder.RenameColumn(
                name: "tier1Wins",
                table: "Users",
                newName: "Tier1Wins");

            migrationBuilder.RenameColumn(
                name: "smorcWins",
                table: "Users",
                newName: "SmorcWins");

            migrationBuilder.RenameColumn(
                name: "rank5Applied",
                table: "Users",
                newName: "Rank5Applied");

            migrationBuilder.RenameColumn(
                name: "rank4Applied",
                table: "Users",
                newName: "Rank4Applied");

            migrationBuilder.RenameColumn(
                name: "rank3Applied",
                table: "Users",
                newName: "Rank3Applied");

            migrationBuilder.RenameColumn(
                name: "rank2Applied",
                table: "Users",
                newName: "Rank2Applied");

            migrationBuilder.RenameColumn(
                name: "rank1Applied",
                table: "Users",
                newName: "Rank1Applied");

            migrationBuilder.RenameColumn(
                name: "prestigeLevel",
                table: "Users",
                newName: "PrestigeLevel");

            migrationBuilder.RenameColumn(
                name: "pointsWon",
                table: "Users",
                newName: "PointsWon");

            migrationBuilder.RenameColumn(
                name: "pointsLost",
                table: "Users",
                newName: "PointsLost");

            migrationBuilder.RenameColumn(
                name: "pointsLastClaimed",
                table: "Users",
                newName: "PointsLastClaimed");

            migrationBuilder.RenameColumn(
                name: "pointsGambled",
                table: "Users",
                newName: "PointsGambled");

            migrationBuilder.RenameColumn(
                name: "pointsClaimedThisStream",
                table: "Users",
                newName: "PointsClaimedThisStream");

            migrationBuilder.RenameColumn(
                name: "points",
                table: "Users",
                newName: "Points");

            migrationBuilder.RenameColumn(
                name: "minutesWatchedThisWeek",
                table: "Users",
                newName: "MinutesWatchedThisWeek");

            migrationBuilder.RenameColumn(
                name: "minutesWatchedThisStream",
                table: "Users",
                newName: "MinutesWatchedThisStream");

            migrationBuilder.RenameColumn(
                name: "minutesWatchedThisMonth",
                table: "Users",
                newName: "MinutesWatchedThisMonth");

            migrationBuilder.RenameColumn(
                name: "minutesInStream",
                table: "Users",
                newName: "MinutesInStream");

            migrationBuilder.RenameColumn(
                name: "marblesWins",
                table: "Users",
                newName: "MarblesWins");

            migrationBuilder.RenameColumn(
                name: "lastSeenDate",
                table: "Users",
                newName: "LastSeenDate");

            migrationBuilder.RenameColumn(
                name: "jackpotWins",
                table: "Users",
                newName: "JackpotWins");

            migrationBuilder.RenameColumn(
                name: "isSuperMod",
                table: "Users",
                newName: "IsSuperMod");

            migrationBuilder.RenameColumn(
                name: "isSub",
                table: "Users",
                newName: "IsSub");

            migrationBuilder.RenameColumn(
                name: "inStream",
                table: "Users",
                newName: "InStream");

            migrationBuilder.RenameColumn(
                name: "highestStreak",
                table: "Users",
                newName: "HighestStreak");

            migrationBuilder.RenameColumn(
                name: "giftedSubsThisMonth",
                table: "Users",
                newName: "GiftedSubsThisMonth");

            migrationBuilder.RenameColumn(
                name: "discordXp",
                table: "Users",
                newName: "DiscordXp");

            migrationBuilder.RenameColumn(
                name: "discordUserId",
                table: "Users",
                newName: "DiscordUserId");

            migrationBuilder.RenameColumn(
                name: "discordLevelUpNotifsEnabled",
                table: "Users",
                newName: "DiscordLevelUpNotifsEnabled");

            migrationBuilder.RenameColumn(
                name: "discordLevel",
                table: "Users",
                newName: "DiscordLevel");

            migrationBuilder.RenameColumn(
                name: "discordDailyTotalClaims",
                table: "Users",
                newName: "DiscordDailyTotalClaims");

            migrationBuilder.RenameColumn(
                name: "discordDailyStreak",
                table: "Users",
                newName: "DiscordDailyStreak");

            migrationBuilder.RenameColumn(
                name: "discordDailyClaimed",
                table: "Users",
                newName: "DiscordDailyClaimed");

            migrationBuilder.RenameColumn(
                name: "diceRolls",
                table: "Users",
                newName: "DiceRolls");

            migrationBuilder.RenameColumn(
                name: "currentStreak",
                table: "Users",
                newName: "CurrentStreak");

            migrationBuilder.RenameColumn(
                name: "bossesPointsWon",
                table: "Users",
                newName: "BossesPointsWon");

            migrationBuilder.RenameColumn(
                name: "bossesDone",
                table: "Users",
                newName: "BossesDone");

            migrationBuilder.RenameColumn(
                name: "bonusDiceRolls",
                table: "Users",
                newName: "BonusDiceRolls");

            migrationBuilder.RenameColumn(
                name: "bitsDonatedThisMonth",
                table: "Users",
                newName: "BitsDonatedThisMonth");

            migrationBuilder.RenameColumn(
                name: "twitchUserId",
                table: "Users",
                newName: "TwitchUserId");

            migrationBuilder.RenameColumn(
                name: "subsGifted",
                table: "Subathon",
                newName: "SubsGifted");

            migrationBuilder.RenameColumn(
                name: "bitsDonated",
                table: "Subathon",
                newName: "BitsDonated");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Subathon",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "uptime",
                table: "StreamStats",
                newName: "Uptime");

            migrationBuilder.RenameColumn(
                name: "uniquePeople",
                table: "StreamStats",
                newName: "UniquePeople");

            migrationBuilder.RenameColumn(
                name: "totalUsersClaimed",
                table: "StreamStats",
                newName: "TotalUsersClaimed");

            migrationBuilder.RenameColumn(
                name: "totalTimeouts",
                table: "StreamStats",
                newName: "TotalTimeouts");

            migrationBuilder.RenameColumn(
                name: "totalSpins",
                table: "StreamStats",
                newName: "TotalSpins");

            migrationBuilder.RenameColumn(
                name: "totalPointsClaimed",
                table: "StreamStats",
                newName: "TotalPointsClaimed");

            migrationBuilder.RenameColumn(
                name: "totalBans",
                table: "StreamStats",
                newName: "TotalBans");

            migrationBuilder.RenameColumn(
                name: "streamStarted",
                table: "StreamStats",
                newName: "StreamStarted");

            migrationBuilder.RenameColumn(
                name: "streamEnded",
                table: "StreamStats",
                newName: "StreamEnded");

            migrationBuilder.RenameColumn(
                name: "startingSubscriberCount",
                table: "StreamStats",
                newName: "StartingSubscriberCount");

            migrationBuilder.RenameColumn(
                name: "startingFollowerCount",
                table: "StreamStats",
                newName: "StartingFollowerCount");

            migrationBuilder.RenameColumn(
                name: "songRequestsSent",
                table: "StreamStats",
                newName: "SongRequestsSent");

            migrationBuilder.RenameColumn(
                name: "songRequestsLiked",
                table: "StreamStats",
                newName: "SongRequestsLiked");

            migrationBuilder.RenameColumn(
                name: "songRequestsBlacklisted",
                table: "StreamStats",
                newName: "SongRequestsBlacklisted");

            migrationBuilder.RenameColumn(
                name: "smorcWins",
                table: "StreamStats",
                newName: "SmorcWins");

            migrationBuilder.RenameColumn(
                name: "rewardRedeemCost",
                table: "StreamStats",
                newName: "RewardRedeemCost");

            migrationBuilder.RenameColumn(
                name: "pointsWon",
                table: "StreamStats",
                newName: "PointsWon");

            migrationBuilder.RenameColumn(
                name: "pointsLost",
                table: "StreamStats",
                newName: "PointsLost");

            migrationBuilder.RenameColumn(
                name: "pointsGambled",
                table: "StreamStats",
                newName: "PointsGambled");

            migrationBuilder.RenameColumn(
                name: "pointsGainedWatching",
                table: "StreamStats",
                newName: "PointsGainedWatching");

            migrationBuilder.RenameColumn(
                name: "pointsGainedSubscribing",
                table: "StreamStats",
                newName: "PointsGainedSubscribing");

            migrationBuilder.RenameColumn(
                name: "peakViewerCount",
                table: "StreamStats",
                newName: "PeakViewerCount");

            migrationBuilder.RenameColumn(
                name: "newSubscribers",
                table: "StreamStats",
                newName: "NewSubscribers");

            migrationBuilder.RenameColumn(
                name: "newGiftedSubs",
                table: "StreamStats",
                newName: "NewGiftedSubs");

            migrationBuilder.RenameColumn(
                name: "newFollowers",
                table: "StreamStats",
                newName: "NewFollowers");

            migrationBuilder.RenameColumn(
                name: "messagesReceived",
                table: "StreamStats",
                newName: "MessagesReceived");

            migrationBuilder.RenameColumn(
                name: "lulWins",
                table: "StreamStats",
                newName: "LulWins");

            migrationBuilder.RenameColumn(
                name: "kappaWins",
                table: "StreamStats",
                newName: "KappaWins");

            migrationBuilder.RenameColumn(
                name: "jackpotWins",
                table: "StreamStats",
                newName: "JackpotWins");

            migrationBuilder.RenameColumn(
                name: "giftedPoints",
                table: "StreamStats",
                newName: "GiftedPoints");

            migrationBuilder.RenameColumn(
                name: "foreheadWins",
                table: "StreamStats",
                newName: "ForeheadWins");

            migrationBuilder.RenameColumn(
                name: "endingSubscriberCount",
                table: "StreamStats",
                newName: "EndingSubscriberCount");

            migrationBuilder.RenameColumn(
                name: "endingFollowerCount",
                table: "StreamStats",
                newName: "EndingFollowerCount");

            migrationBuilder.RenameColumn(
                name: "discordRanksEarnt",
                table: "StreamStats",
                newName: "DiscordRanksEarnt");

            migrationBuilder.RenameColumn(
                name: "commandsSent",
                table: "StreamStats",
                newName: "CommandsSent");

            migrationBuilder.RenameColumn(
                name: "bitsDonated",
                table: "StreamStats",
                newName: "BitsDonated");

            migrationBuilder.RenameColumn(
                name: "avgViewCount",
                table: "StreamStats",
                newName: "AvgViewCount");

            migrationBuilder.RenameColumn(
                name: "amountOfUsersReset",
                table: "StreamStats",
                newName: "AmountOfUsersReset");

            migrationBuilder.RenameColumn(
                name: "amountOfRewardsRedeemed",
                table: "StreamStats",
                newName: "AmountOfRewardsRedeemed");

            migrationBuilder.RenameColumn(
                name: "amountOfDiscordUsersJoined",
                table: "StreamStats",
                newName: "AmountOfDiscordUsersJoined");

            migrationBuilder.RenameColumn(
                name: "streamId",
                table: "StreamStats",
                newName: "StreamId");

            migrationBuilder.RenameColumn(
                name: "totalSpins",
                table: "SlotMachine",
                newName: "TotalSpins");

            migrationBuilder.RenameColumn(
                name: "tier3Wins",
                table: "SlotMachine",
                newName: "Tier3Wins");

            migrationBuilder.RenameColumn(
                name: "tier2Wins",
                table: "SlotMachine",
                newName: "Tier2Wins");

            migrationBuilder.RenameColumn(
                name: "tier1Wins",
                table: "SlotMachine",
                newName: "Tier1Wins");

            migrationBuilder.RenameColumn(
                name: "smorcWins",
                table: "SlotMachine",
                newName: "SmorcWins");

            migrationBuilder.RenameColumn(
                name: "pineappleWins",
                table: "SlotMachine",
                newName: "PineappleWins");

            migrationBuilder.RenameColumn(
                name: "jackpotWins",
                table: "SlotMachine",
                newName: "JackpotWins");

            migrationBuilder.RenameColumn(
                name: "jackpotAmount",
                table: "SlotMachine",
                newName: "JackpotAmount");

            migrationBuilder.RenameColumn(
                name: "grapesWins",
                table: "SlotMachine",
                newName: "GrapesWins");

            migrationBuilder.RenameColumn(
                name: "eggplantWins",
                table: "SlotMachine",
                newName: "EggplantWins");

            migrationBuilder.RenameColumn(
                name: "discordTotalSpins",
                table: "SlotMachine",
                newName: "DiscordTotalSpins");

            migrationBuilder.RenameColumn(
                name: "cucumberWins",
                table: "SlotMachine",
                newName: "CucumberWins");

            migrationBuilder.RenameColumn(
                name: "cherriesWins",
                table: "SlotMachine",
                newName: "CherriesWins");

            migrationBuilder.RenameColumn(
                name: "cheeseWins",
                table: "SlotMachine",
                newName: "CheeseWins");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "RankBeggar",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "RankBeggar",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "discordID",
                table: "DiscordLinkRequests",
                newName: "DiscordID");

            migrationBuilder.RenameColumn(
                name: "subathonTime",
                table: "Config",
                newName: "SubathonTime");

            migrationBuilder.RenameColumn(
                name: "streamAnnounced",
                table: "Config",
                newName: "StreamAnnounced");

            migrationBuilder.RenameColumn(
                name: "pinnedStreamMessageId",
                table: "Config",
                newName: "PinnedStreamMessageId");

            migrationBuilder.RenameColumn(
                name: "pinnedStreamMessage",
                table: "Config",
                newName: "PinnedStreamMessage");

            migrationBuilder.RenameColumn(
                name: "pinnedStreamDate",
                table: "Config",
                newName: "PinnedStreamDate");

            migrationBuilder.RenameColumn(
                name: "lastDailyPointsAllowed",
                table: "Config",
                newName: "LastDailyPointsAllowed");

            migrationBuilder.RenameColumn(
                name: "dailyPointsCollectingAllowed",
                table: "Config",
                newName: "DailyPointsCollectingAllowed");

            migrationBuilder.RenameColumn(
                name: "broadcasterRefresh",
                table: "Config",
                newName: "BroadcasterRefresh");

            migrationBuilder.RenameColumn(
                name: "broadcasterOAuth",
                table: "Config",
                newName: "BroadcasterOAuth");

            migrationBuilder.RenameColumn(
                name: "broadcasterName",
                table: "Config",
                newName: "BroadcasterName");

            migrationBuilder.RenameColumn(
                name: "timesUsed",
                table: "Commands",
                newName: "TimesUsed");

            migrationBuilder.RenameColumn(
                name: "lastUsed",
                table: "Commands",
                newName: "LastUsed");

            migrationBuilder.RenameColumn(
                name: "commandText",
                table: "Commands",
                newName: "CommandText");

            migrationBuilder.RenameColumn(
                name: "commandName",
                table: "Commands",
                newName: "CommandName");

            migrationBuilder.RenameColumn(
                name: "month",
                table: "Birthdays",
                newName: "Month");

            migrationBuilder.RenameColumn(
                name: "day",
                table: "Birthdays",
                newName: "Day");

            migrationBuilder.AddColumn<string>(
                name: "BotName",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BotOAuth",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DiscordAPIKey",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordBanRole",
                table: "Config",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordCommandsChannelID",
                table: "Config",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordEventChannelID",
                table: "Config",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordGeneralChannel",
                table: "Config",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordGiveawayChannelID",
                table: "Config",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordGuildID",
                table: "Config",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordGuildOwner",
                table: "Config",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordLinkingChannelID",
                table: "Config",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordRankUpAnnouncementChannelID",
                table: "Config",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordReactionRoleChannelID",
                table: "Config",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordSocksChannelID",
                table: "Config",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordStreamAnnouncementChannelID",
                table: "Config",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PointsName",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "PrestigeCap",
                table: "Config",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "TwitchAPIClientID",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TwitchAPISecret",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TwitchChannelID",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BotName",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "BotOAuth",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "DiscordAPIKey",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "DiscordBanRole",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "DiscordCommandsChannelID",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "DiscordEventChannelID",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "DiscordGeneralChannel",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "DiscordGiveawayChannelID",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "DiscordGuildID",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "DiscordGuildOwner",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "DiscordLinkingChannelID",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "DiscordRankUpAnnouncementChannelID",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "DiscordReactionRoleChannelID",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "DiscordSocksChannelID",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "DiscordStreamAnnouncementChannelID",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "PointsName",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "PrestigeCap",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "TwitchAPIClientID",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "TwitchAPISecret",
                table: "Config");

            migrationBuilder.DropColumn(
                name: "TwitchChannelID",
                table: "Config");

            migrationBuilder.RenameColumn(
                name: "WarnStrikes",
                table: "Users",
                newName: "warnStrikes");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Users",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "TotalTimesClaimed",
                table: "Users",
                newName: "totalTimesClaimed");

            migrationBuilder.RenameColumn(
                name: "TotalSpins",
                table: "Users",
                newName: "totalSpins");

            migrationBuilder.RenameColumn(
                name: "TotalPointsClaimed",
                table: "Users",
                newName: "totalPointsClaimed");

            migrationBuilder.RenameColumn(
                name: "TotalMessages",
                table: "Users",
                newName: "totalMessages");

            migrationBuilder.RenameColumn(
                name: "TimeoutStrikes",
                table: "Users",
                newName: "timeoutStrikes");

            migrationBuilder.RenameColumn(
                name: "Tier3Wins",
                table: "Users",
                newName: "tier3Wins");

            migrationBuilder.RenameColumn(
                name: "Tier2Wins",
                table: "Users",
                newName: "tier2Wins");

            migrationBuilder.RenameColumn(
                name: "Tier1Wins",
                table: "Users",
                newName: "tier1Wins");

            migrationBuilder.RenameColumn(
                name: "SmorcWins",
                table: "Users",
                newName: "smorcWins");

            migrationBuilder.RenameColumn(
                name: "Rank5Applied",
                table: "Users",
                newName: "rank5Applied");

            migrationBuilder.RenameColumn(
                name: "Rank4Applied",
                table: "Users",
                newName: "rank4Applied");

            migrationBuilder.RenameColumn(
                name: "Rank3Applied",
                table: "Users",
                newName: "rank3Applied");

            migrationBuilder.RenameColumn(
                name: "Rank2Applied",
                table: "Users",
                newName: "rank2Applied");

            migrationBuilder.RenameColumn(
                name: "Rank1Applied",
                table: "Users",
                newName: "rank1Applied");

            migrationBuilder.RenameColumn(
                name: "PrestigeLevel",
                table: "Users",
                newName: "prestigeLevel");

            migrationBuilder.RenameColumn(
                name: "PointsWon",
                table: "Users",
                newName: "pointsWon");

            migrationBuilder.RenameColumn(
                name: "PointsLost",
                table: "Users",
                newName: "pointsLost");

            migrationBuilder.RenameColumn(
                name: "PointsLastClaimed",
                table: "Users",
                newName: "pointsLastClaimed");

            migrationBuilder.RenameColumn(
                name: "PointsGambled",
                table: "Users",
                newName: "pointsGambled");

            migrationBuilder.RenameColumn(
                name: "PointsClaimedThisStream",
                table: "Users",
                newName: "pointsClaimedThisStream");

            migrationBuilder.RenameColumn(
                name: "Points",
                table: "Users",
                newName: "points");

            migrationBuilder.RenameColumn(
                name: "MinutesWatchedThisWeek",
                table: "Users",
                newName: "minutesWatchedThisWeek");

            migrationBuilder.RenameColumn(
                name: "MinutesWatchedThisStream",
                table: "Users",
                newName: "minutesWatchedThisStream");

            migrationBuilder.RenameColumn(
                name: "MinutesWatchedThisMonth",
                table: "Users",
                newName: "minutesWatchedThisMonth");

            migrationBuilder.RenameColumn(
                name: "MinutesInStream",
                table: "Users",
                newName: "minutesInStream");

            migrationBuilder.RenameColumn(
                name: "MarblesWins",
                table: "Users",
                newName: "marblesWins");

            migrationBuilder.RenameColumn(
                name: "LastSeenDate",
                table: "Users",
                newName: "lastSeenDate");

            migrationBuilder.RenameColumn(
                name: "JackpotWins",
                table: "Users",
                newName: "jackpotWins");

            migrationBuilder.RenameColumn(
                name: "IsSuperMod",
                table: "Users",
                newName: "isSuperMod");

            migrationBuilder.RenameColumn(
                name: "IsSub",
                table: "Users",
                newName: "isSub");

            migrationBuilder.RenameColumn(
                name: "InStream",
                table: "Users",
                newName: "inStream");

            migrationBuilder.RenameColumn(
                name: "HighestStreak",
                table: "Users",
                newName: "highestStreak");

            migrationBuilder.RenameColumn(
                name: "GiftedSubsThisMonth",
                table: "Users",
                newName: "giftedSubsThisMonth");

            migrationBuilder.RenameColumn(
                name: "DiscordXp",
                table: "Users",
                newName: "discordXp");

            migrationBuilder.RenameColumn(
                name: "DiscordUserId",
                table: "Users",
                newName: "discordUserId");

            migrationBuilder.RenameColumn(
                name: "DiscordLevelUpNotifsEnabled",
                table: "Users",
                newName: "discordLevelUpNotifsEnabled");

            migrationBuilder.RenameColumn(
                name: "DiscordLevel",
                table: "Users",
                newName: "discordLevel");

            migrationBuilder.RenameColumn(
                name: "DiscordDailyTotalClaims",
                table: "Users",
                newName: "discordDailyTotalClaims");

            migrationBuilder.RenameColumn(
                name: "DiscordDailyStreak",
                table: "Users",
                newName: "discordDailyStreak");

            migrationBuilder.RenameColumn(
                name: "DiscordDailyClaimed",
                table: "Users",
                newName: "discordDailyClaimed");

            migrationBuilder.RenameColumn(
                name: "DiceRolls",
                table: "Users",
                newName: "diceRolls");

            migrationBuilder.RenameColumn(
                name: "CurrentStreak",
                table: "Users",
                newName: "currentStreak");

            migrationBuilder.RenameColumn(
                name: "BossesPointsWon",
                table: "Users",
                newName: "bossesPointsWon");

            migrationBuilder.RenameColumn(
                name: "BossesDone",
                table: "Users",
                newName: "bossesDone");

            migrationBuilder.RenameColumn(
                name: "BonusDiceRolls",
                table: "Users",
                newName: "bonusDiceRolls");

            migrationBuilder.RenameColumn(
                name: "BitsDonatedThisMonth",
                table: "Users",
                newName: "bitsDonatedThisMonth");

            migrationBuilder.RenameColumn(
                name: "TwitchUserId",
                table: "Users",
                newName: "twitchUserId");

            migrationBuilder.RenameColumn(
                name: "SubsGifted",
                table: "Subathon",
                newName: "subsGifted");

            migrationBuilder.RenameColumn(
                name: "BitsDonated",
                table: "Subathon",
                newName: "bitsDonated");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Subathon",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Uptime",
                table: "StreamStats",
                newName: "uptime");

            migrationBuilder.RenameColumn(
                name: "UniquePeople",
                table: "StreamStats",
                newName: "uniquePeople");

            migrationBuilder.RenameColumn(
                name: "TotalUsersClaimed",
                table: "StreamStats",
                newName: "totalUsersClaimed");

            migrationBuilder.RenameColumn(
                name: "TotalTimeouts",
                table: "StreamStats",
                newName: "totalTimeouts");

            migrationBuilder.RenameColumn(
                name: "TotalSpins",
                table: "StreamStats",
                newName: "totalSpins");

            migrationBuilder.RenameColumn(
                name: "TotalPointsClaimed",
                table: "StreamStats",
                newName: "totalPointsClaimed");

            migrationBuilder.RenameColumn(
                name: "TotalBans",
                table: "StreamStats",
                newName: "totalBans");

            migrationBuilder.RenameColumn(
                name: "StreamStarted",
                table: "StreamStats",
                newName: "streamStarted");

            migrationBuilder.RenameColumn(
                name: "StreamEnded",
                table: "StreamStats",
                newName: "streamEnded");

            migrationBuilder.RenameColumn(
                name: "StartingSubscriberCount",
                table: "StreamStats",
                newName: "startingSubscriberCount");

            migrationBuilder.RenameColumn(
                name: "StartingFollowerCount",
                table: "StreamStats",
                newName: "startingFollowerCount");

            migrationBuilder.RenameColumn(
                name: "SongRequestsSent",
                table: "StreamStats",
                newName: "songRequestsSent");

            migrationBuilder.RenameColumn(
                name: "SongRequestsLiked",
                table: "StreamStats",
                newName: "songRequestsLiked");

            migrationBuilder.RenameColumn(
                name: "SongRequestsBlacklisted",
                table: "StreamStats",
                newName: "songRequestsBlacklisted");

            migrationBuilder.RenameColumn(
                name: "SmorcWins",
                table: "StreamStats",
                newName: "smorcWins");

            migrationBuilder.RenameColumn(
                name: "RewardRedeemCost",
                table: "StreamStats",
                newName: "rewardRedeemCost");

            migrationBuilder.RenameColumn(
                name: "PointsWon",
                table: "StreamStats",
                newName: "pointsWon");

            migrationBuilder.RenameColumn(
                name: "PointsLost",
                table: "StreamStats",
                newName: "pointsLost");

            migrationBuilder.RenameColumn(
                name: "PointsGambled",
                table: "StreamStats",
                newName: "pointsGambled");

            migrationBuilder.RenameColumn(
                name: "PointsGainedWatching",
                table: "StreamStats",
                newName: "pointsGainedWatching");

            migrationBuilder.RenameColumn(
                name: "PointsGainedSubscribing",
                table: "StreamStats",
                newName: "pointsGainedSubscribing");

            migrationBuilder.RenameColumn(
                name: "PeakViewerCount",
                table: "StreamStats",
                newName: "peakViewerCount");

            migrationBuilder.RenameColumn(
                name: "NewSubscribers",
                table: "StreamStats",
                newName: "newSubscribers");

            migrationBuilder.RenameColumn(
                name: "NewGiftedSubs",
                table: "StreamStats",
                newName: "newGiftedSubs");

            migrationBuilder.RenameColumn(
                name: "NewFollowers",
                table: "StreamStats",
                newName: "newFollowers");

            migrationBuilder.RenameColumn(
                name: "MessagesReceived",
                table: "StreamStats",
                newName: "messagesReceived");

            migrationBuilder.RenameColumn(
                name: "LulWins",
                table: "StreamStats",
                newName: "lulWins");

            migrationBuilder.RenameColumn(
                name: "KappaWins",
                table: "StreamStats",
                newName: "kappaWins");

            migrationBuilder.RenameColumn(
                name: "JackpotWins",
                table: "StreamStats",
                newName: "jackpotWins");

            migrationBuilder.RenameColumn(
                name: "GiftedPoints",
                table: "StreamStats",
                newName: "giftedPoints");

            migrationBuilder.RenameColumn(
                name: "ForeheadWins",
                table: "StreamStats",
                newName: "foreheadWins");

            migrationBuilder.RenameColumn(
                name: "EndingSubscriberCount",
                table: "StreamStats",
                newName: "endingSubscriberCount");

            migrationBuilder.RenameColumn(
                name: "EndingFollowerCount",
                table: "StreamStats",
                newName: "endingFollowerCount");

            migrationBuilder.RenameColumn(
                name: "DiscordRanksEarnt",
                table: "StreamStats",
                newName: "discordRanksEarnt");

            migrationBuilder.RenameColumn(
                name: "CommandsSent",
                table: "StreamStats",
                newName: "commandsSent");

            migrationBuilder.RenameColumn(
                name: "BitsDonated",
                table: "StreamStats",
                newName: "bitsDonated");

            migrationBuilder.RenameColumn(
                name: "AvgViewCount",
                table: "StreamStats",
                newName: "avgViewCount");

            migrationBuilder.RenameColumn(
                name: "AmountOfUsersReset",
                table: "StreamStats",
                newName: "amountOfUsersReset");

            migrationBuilder.RenameColumn(
                name: "AmountOfRewardsRedeemed",
                table: "StreamStats",
                newName: "amountOfRewardsRedeemed");

            migrationBuilder.RenameColumn(
                name: "AmountOfDiscordUsersJoined",
                table: "StreamStats",
                newName: "amountOfDiscordUsersJoined");

            migrationBuilder.RenameColumn(
                name: "StreamId",
                table: "StreamStats",
                newName: "streamId");

            migrationBuilder.RenameColumn(
                name: "TotalSpins",
                table: "SlotMachine",
                newName: "totalSpins");

            migrationBuilder.RenameColumn(
                name: "Tier3Wins",
                table: "SlotMachine",
                newName: "tier3Wins");

            migrationBuilder.RenameColumn(
                name: "Tier2Wins",
                table: "SlotMachine",
                newName: "tier2Wins");

            migrationBuilder.RenameColumn(
                name: "Tier1Wins",
                table: "SlotMachine",
                newName: "tier1Wins");

            migrationBuilder.RenameColumn(
                name: "SmorcWins",
                table: "SlotMachine",
                newName: "smorcWins");

            migrationBuilder.RenameColumn(
                name: "PineappleWins",
                table: "SlotMachine",
                newName: "pineappleWins");

            migrationBuilder.RenameColumn(
                name: "JackpotWins",
                table: "SlotMachine",
                newName: "jackpotWins");

            migrationBuilder.RenameColumn(
                name: "JackpotAmount",
                table: "SlotMachine",
                newName: "jackpotAmount");

            migrationBuilder.RenameColumn(
                name: "GrapesWins",
                table: "SlotMachine",
                newName: "grapesWins");

            migrationBuilder.RenameColumn(
                name: "EggplantWins",
                table: "SlotMachine",
                newName: "eggplantWins");

            migrationBuilder.RenameColumn(
                name: "DiscordTotalSpins",
                table: "SlotMachine",
                newName: "discordTotalSpins");

            migrationBuilder.RenameColumn(
                name: "CucumberWins",
                table: "SlotMachine",
                newName: "cucumberWins");

            migrationBuilder.RenameColumn(
                name: "CherriesWins",
                table: "SlotMachine",
                newName: "cherriesWins");

            migrationBuilder.RenameColumn(
                name: "CheeseWins",
                table: "SlotMachine",
                newName: "cheeseWins");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "RankBeggar",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "RankBeggar",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "DiscordID",
                table: "DiscordLinkRequests",
                newName: "discordID");

            migrationBuilder.RenameColumn(
                name: "SubathonTime",
                table: "Config",
                newName: "subathonTime");

            migrationBuilder.RenameColumn(
                name: "StreamAnnounced",
                table: "Config",
                newName: "streamAnnounced");

            migrationBuilder.RenameColumn(
                name: "PinnedStreamMessageId",
                table: "Config",
                newName: "pinnedStreamMessageId");

            migrationBuilder.RenameColumn(
                name: "PinnedStreamMessage",
                table: "Config",
                newName: "pinnedStreamMessage");

            migrationBuilder.RenameColumn(
                name: "PinnedStreamDate",
                table: "Config",
                newName: "pinnedStreamDate");

            migrationBuilder.RenameColumn(
                name: "LastDailyPointsAllowed",
                table: "Config",
                newName: "lastDailyPointsAllowed");

            migrationBuilder.RenameColumn(
                name: "DailyPointsCollectingAllowed",
                table: "Config",
                newName: "dailyPointsCollectingAllowed");

            migrationBuilder.RenameColumn(
                name: "BroadcasterRefresh",
                table: "Config",
                newName: "broadcasterRefresh");

            migrationBuilder.RenameColumn(
                name: "BroadcasterOAuth",
                table: "Config",
                newName: "broadcasterOAuth");

            migrationBuilder.RenameColumn(
                name: "BroadcasterName",
                table: "Config",
                newName: "broadcasterName");

            migrationBuilder.RenameColumn(
                name: "TimesUsed",
                table: "Commands",
                newName: "timesUsed");

            migrationBuilder.RenameColumn(
                name: "LastUsed",
                table: "Commands",
                newName: "lastUsed");

            migrationBuilder.RenameColumn(
                name: "CommandText",
                table: "Commands",
                newName: "commandText");

            migrationBuilder.RenameColumn(
                name: "CommandName",
                table: "Commands",
                newName: "commandName");

            migrationBuilder.RenameColumn(
                name: "Month",
                table: "Birthdays",
                newName: "month");

            migrationBuilder.RenameColumn(
                name: "Day",
                table: "Birthdays",
                newName: "day");
        }
    }
}