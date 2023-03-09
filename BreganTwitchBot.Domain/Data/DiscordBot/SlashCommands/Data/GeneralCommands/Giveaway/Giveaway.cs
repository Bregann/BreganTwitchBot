using BreganTwitchBot.Domain.Data.DiscordBot;
using BreganTwitchBot.Infrastructure.Database.Context;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.GeneralCommands.Giveaway
{
    public class GiveawayData
    { //todo: redo the giveaway
        public static async Task<string> HandleGiveawayCommand(SocketInteractionContext ctx, ulong msgId)
        {
            var channel = DiscordConnection.DiscordClient.GetChannel(AppConfig.DiscordGiveawayChannelID) as IMessageChannel;

            if (ctx.Channel.Id != 622869364076707862 || msgId == 0)
            {
                return "lol";
            }

            var msg = channel.GetMessageAsync(msgId);
            var emote = await DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID).GetEmoteAsync(AppConfig.DiscordGiveawayChannelID);

            var reactions = msg.Result.GetReactionUsersAsync(emote, 500).Flatten();
            var list = await (from item in reactions select item.Id).ToListAsync();
            list.Remove(219623957957967872);

            var duplicateList = list.ToList();
            var basePeople = GetLinkedPeople(2800);
            var hours200 = GetLinkedPeople(12000);
            var hours300 = GetLinkedPeople(18000);
            var hours400 = GetLinkedPeople(24000);
            var hours500 = GetLinkedPeople(30000);
            var hours600 = GetLinkedPeople(36000);
            var hours700 = GetLinkedPeople(42000);
            var hours800 = GetLinkedPeople(48000);

            foreach (var user in duplicateList)
            {
                if (basePeople.Contains(user))
                {
                    list.Add(user);
                }
                else
                {
                    list.Remove(user);
                }

                if (hours200.Contains(user))
                {
                    list.Add(user);
                }

                if (hours300.Contains(user))
                {
                    list.Add(user);
                    list.Add(user);
                    list.Add(user);
                }

                if (hours400.Contains(user))
                {
                    list.Add(user);
                    list.Add(user);
                    list.Add(user);
                }

                if (hours500.Contains(user))
                {
                    list.Add(user);
                    list.Add(user);
                    list.Add(user);
                }

                if (hours600.Contains(user))
                {
                    list.Add(user);
                    list.Add(user);
                    list.Add(user);
                }

                if (hours700.Contains(user))
                {
                    list.Add(user);
                    list.Add(user);
                    list.Add(user);
                }

                if (hours800.Contains(user))
                {
                    list.Add(user);
                    list.Add(user);
                    list.Add(user);
                }
            }

            var random = new Random().Next(0, list.Count);
            return $"The winner of the giveaway is... <@{list[random]}>!";
        }

        public static async Task<string> HandleGiveawayButton(ulong userIdWhoPressed, ulong interactionId)
        {
            var channel = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordGiveawayChannelID) as IMessageChannel;

            if (userIdWhoPressed != AppConfig.DiscordGuildOwner || interactionId == 0)
            {
                return "lol";
            }

            var msg = channel.GetMessageAsync(interactionId);
            var emote = await DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID).GetEmoteAsync(384735587552198662);

            var reactions = msg.Result.GetReactionUsersAsync(emote, 500).Flatten();
            var list = await (from item in reactions select item.Id).ToListAsync();

            var duplicateList = list.ToList();
            var basePeople = GetLinkedPeople(2500);
            var hours200 = GetLinkedPeople(12000);
            var hours300 = GetLinkedPeople(18000);
            var hours400 = GetLinkedPeople(24000);
            var hours500 = GetLinkedPeople(30000);

            foreach (var user in duplicateList)
            {
                if (basePeople.Contains(user))
                {
                    list.Add(user);
                }
                else
                {
                    list.Remove(user);
                }

                if (hours200.Contains(user))
                {
                    list.Add(user);
                }

                if (hours300.Contains(user))
                {
                    list.Add(user);
                }

                if (hours400.Contains(user))
                {
                    list.Add(user);
                }

                if (hours500.Contains(user))
                {
                    list.Add(user);
                }
            }

            var random = new Random().Next(0, list.Count);
            return $"The winner of the giveaway is... <@{list[random]}>!";
        }

        private static List<ulong> GetLinkedPeople(int minutesWatched)
        {
            using (var context = new DatabaseContext())
            {
                return context.Users.Where(x => x.MinutesInStream >= minutesWatched && x.DiscordUserId != 0 && x.LastSeenDate > DateTime.UtcNow.AddDays(-30)).Select(x => x.DiscordUserId).ToList();
            }
        }
    }
}