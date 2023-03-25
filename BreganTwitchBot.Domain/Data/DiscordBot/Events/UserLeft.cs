using BreganTwitchBot.Infrastructure.Database.Context;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Data.DiscordBot.Events
{
    public class UserLeftEvent
    {
        public static async Task UnlinkUserFromDiscordAndAlert(SocketUser user)
        {
            var messageEmbed = new EmbedBuilder()
            {
                Title = "User Left",
                Timestamp = DateTime.Now,
                Color = new Color(255, 112, 51)
            };

            using (var context = new DatabaseContext())
            {
                var userFromDb = context.Users.Include(x => x.Watchtime).Where(x => x.DiscordUserId == user.Id).FirstOrDefault();
                if (userFromDb == null)
                {
                    messageEmbed.AddField("Was Linked?", "false", true);
                    messageEmbed.AddField("Twitch username", "n/a", true);
                }
                else
                {
                    var userTime = TimeSpan.FromMinutes(userFromDb.Watchtime.MinutesInStream);

                    messageEmbed.AddField("Twitch username", userFromDb.Username);
                    messageEmbed.AddField("Minutes watched", userTime.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7), true);
                    messageEmbed.AddField("Last in stream", userFromDb.LastSeenDate, true);

                    userFromDb.DiscordUserId = 0;
                    await context.SaveChangesAsync();
                }
            }

            messageEmbed.AddField("Discord username", user.Username, true);



            var channel = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordEventChannelID) as IMessageChannel;

            if (channel != null)
            {
                await channel.SendMessageAsync(embed: messageEmbed.Build());
            }
        }
    }
}
