using BreganTwitchBot.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.Giveaway
{
    public class GiveawayDataDto
    {
        public required string Response { get; set; } = "";
        public required bool Ephemeral { get; set; }
    }

    public class GiveawayData
    {
        public static async Task<GiveawayDataDto> HandleGiveawayButtonPressed(ulong userIdWhoPressed, string interactionId)
        {
            Log.Information($"[Discord Giveaways] Giveaway button pressed  - ID: {userIdWhoPressed} | Interaction ID: {interactionId}");

            if (interactionId.EndsWith("-enter"))
            {
                //Roll the giveaway winner if its the owner
                if (userIdWhoPressed == AppConfig.DiscordGuildOwnerID)
                {
                    using (var context = new DatabaseContext())
                    {
                        var entries = context.DiscordGiveaways.Where(x => x.EligbleToWin == true && x.GiveawayId == interactionId.Replace("-enter", "")).Select(x => x.DiscordUserId).ToList();
                        var rnd = new Random();

                        return new GiveawayDataDto
                        {
                            Response = $"The winner of the giveaway is... <@{entries[rnd.Next(0, entries.Count)]}>",
                            Ephemeral = false
                        };
                    }
                }

                var timesEntered = await AddUserToGiveaway(userIdWhoPressed, interactionId.Replace("-enter", ""));

                if (timesEntered == 0)
                {
                    return new GiveawayDataDto
                    {
                        Response = "don't be silly, you have already entered the giveaway!",
                        Ephemeral = true
                    };
                }
                else
                {
                    return new GiveawayDataDto
                    {
                        Response = $"You have entered the giveaway! You have **{timesEntered}** entries in the giveaway. You can earn more entries by earning watchtime ranks in the stream and levelling up in the Discord. Every 1,000 discord xp will award you with an extra entry (up to 30k xp)",
                        Ephemeral = true
                    };
                }
            }

            if (interactionId.EndsWith("-check"))
            {
                var giveawayId = interactionId.Replace("-check", "");

                if (userIdWhoPressed == AppConfig.DiscordGuildOwnerID)
                {
                    return new GiveawayDataDto { Response = $"There are currently **{GetUsersEntered(giveawayId)}** people in the giveaway with a total of **{GetTotalEntries(giveawayId)}** entries", Ephemeral = true };
                }

                var entries = CheckEntriesForUser(userIdWhoPressed, giveawayId);
                return entries == 0 ? new GiveawayDataDto { Response = "You haven't entered the giveaway yet! Try clicking the other button", Ephemeral = true } : new GiveawayDataDto { Response = $"You have entered the giveaway! You have **{entries}** entries in the giveaway", Ephemeral = true };
            }

            return new GiveawayDataDto { Response = "", Ephemeral = true };
        }

        private static async Task<int> AddUserToGiveaway(ulong userId, string interactionId)
        {
            using (var context = new DatabaseContext())
            {
                //Check if they're already in the giveaway
                if (context.DiscordGiveaways.Any(x => x.GiveawayId == interactionId && x.DiscordUserId == userId))
                {
                    Log.Information($"[Discord Giveaways] User {userId} has already entered the giveaway with id {interactionId}");
                    return 0;
                }

                var user = context.Users.Include(x => x.Watchtime).Include(x => x.DiscordUserStats).Where(x => x.DiscordUserId == userId).First();

                var rankUpsGained = Convert.ToInt32(user.Watchtime.Rank1Applied) + Convert.ToInt32(user.Watchtime.Rank2Applied) + Convert.ToInt32(user.Watchtime.Rank3Applied) + Convert.ToInt32(user.Watchtime.Rank4Applied) + Convert.ToInt32(user.Watchtime.Rank5Applied);
                var discordXpEntries = user.DiscordUserStats.DiscordXp >= 30000 ? 30 : user.DiscordUserStats.DiscordXp / 1000;
                Log.Information($"[Discord Giveaways] User {userId} has {rankUpsGained} extra entries from discord rank ups and {discordXpEntries} from discord xp");

                if (user.Watchtime.MinutesInStream < 3600)
                {
                    var riggedTimesToAdd = 1 + rankUpsGained + discordXpEntries;

                    for (int i = 0; i < riggedTimesToAdd; i++)
                    {
                        context.DiscordGiveaways.Add(new Infrastructure.Database.Models.DiscordGiveaways
                        {
                            DiscordUserId = userId,
                            GiveawayId = interactionId,
                            EligbleToWin = false
                        });
                    }

                    await context.SaveChangesAsync();
                    Log.Information($"[Discord Giveaways] User {userId} has been fake entered");
                    return 1 + rankUpsGained;
                }

                var timesToAdd = (user.Watchtime.MinutesInStream / 3600) + rankUpsGained + +discordXpEntries;

                for (int i = 0; i < timesToAdd; i++)
                {
                    context.DiscordGiveaways.Add(new Infrastructure.Database.Models.DiscordGiveaways
                    {
                        DiscordUserId = userId,
                        GiveawayId = interactionId,
                        EligbleToWin = true
                    });
                }

                await context.SaveChangesAsync();

                Log.Information($"[Discord Giveaways] User {userId} has been entered {timesToAdd} times");
                return timesToAdd;
            }
        }

        private static int CheckEntriesForUser(ulong userId, string interactionId)
        {
            using (var context = new DatabaseContext())
            {
                return context.DiscordGiveaways.Where(x => x.DiscordUserId == userId && x.GiveawayId == interactionId).Count();
            }
        }

        private static int GetUsersEntered(string interactionId)
        {
            using (var context = new DatabaseContext())
            {
                return context.DiscordGiveaways.Where(x => x.GiveawayId == interactionId).Select(x => x.DiscordUserId).Distinct().Count();
            }
        }

        private static int GetTotalEntries(string interactionId)
        {
            using (var context = new DatabaseContext())
            {
                return context.DiscordGiveaways.Where(x => x.GiveawayId == interactionId).Count();
            }
        }
    }
}