using BreganTwitchBot.Data;
using BreganTwitchBot.Services;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Core.DiscordBot.Commands.Modules.GeneralCommands.Giveaway
{
    public class Giveaway
    {
        public static async Task<string> HandleGiveawayCommand(SocketInteractionContext ctx, ulong msgId)
        {
            var channel = DiscordConnection.DiscordClient.GetChannel(Config.DiscordGiveawayChannelID) as IMessageChannel;

            if (ctx.Channel.Id != 622869364076707862 || msgId == 0)
            {
                return "lol";
            }

            var msg = channel.GetMessageAsync(msgId);
            var emote = await DiscordConnection.DiscordClient.GetGuild(Config.DiscordGuildID).GetEmoteAsync(Config.DiscordGiveawayChannelID);

            var reactions = msg.Result.GetReactionUsersAsync(emote, 500).Flatten();
            var list = await (from item in reactions select item.Id).ToListAsync();
            list.Remove(219623957957967872);

            var duplicateList = list.ToList();
            var basePeople = GetLinkedPeople(2200);
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
            }

            var random = new Random().Next(0, list.Count);
            return $"The winner of the giveaway is... <@{list[random]}>!";
        }

        public static async Task<string> HandleGiveawayButton(ulong userIdWhoPressed, ulong interactionId)
        {
            var channel = await DiscordConnection.DiscordClient.GetChannelAsync(Config.DiscordGiveawayChannelID) as IMessageChannel;

            if (userIdWhoPressed != Config.DiscordGuildOwner || interactionId == 0)
            {
                return "lol";
            }

            var msg = channel.GetMessageAsync(interactionId);
            var emote = await DiscordConnection.DiscordClient.GetGuild(Config.DiscordGuildID).GetEmoteAsync(Config.DiscordGiveawayChannelID);

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
                return context.Users.Where(x => x.MinutesInStream >= minutesWatched && x.DiscordUserId != 0 && x.LastSeenDate > DateTime.Now.AddDays(-30)).Select(x => x.DiscordUserId).ToList();
            }
        }
    }
}
