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
                        Response = $"You have entered the giveaway! You have **{timesEntered}** entries in the giveaway. You can earn more entries by watching the stream for longer",
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

                var entries = CheckentriesForUser(userIdWhoPressed, giveawayId);
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

                var user = context.Users.Include(x => x.Watchtime).Where(x => x.DiscordUserId == userId).First();
                var timesToAdd = user.Watchtime.MinutesInStream / 6000;

                if (user.Watchtime.MinutesInStream < 3600)
                {
                    context.DiscordGiveaways.Add(new Infrastructure.Database.Models.DiscordGiveaways
                    {
                        DiscordUserId = userId,
                        GiveawayId = interactionId,
                        EligbleToWin = false
                    });

                    await context.SaveChangesAsync();
                    Log.Information($"[Discord Giveaways] User {userId} has been fake entered");
                    return 1;
                }

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

        private static int CheckentriesForUser(ulong userId, string interactionId)
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
                return context.DiscordGiveaways.Where(x => x.GiveawayId == interactionId).Distinct().Count();
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