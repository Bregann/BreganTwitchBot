using BreganTwitchBot.Domain.Data.DiscordBot;
using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.GeneralCommands.Whois
{
    public class WhoisData
    {
        public static async Task<EmbedBuilder> HandleWhoisCommand(SocketInteractionContext ctx, string twitchUsername, IUser discordUser)
        {
            var isMod = DiscordHelper.IsUserMod(ctx.User.Id);

            //is mot
            if (!isMod)
            {
                return null;
            }

            var embed = new EmbedBuilder()
            {
                Title = "Who is Discord",
                Timestamp = DateTime.Now,
                Color = new Color(238, 255, 46)
            };

            DateTime? timeInStream;
            long totalMessages;

            if (discordUser != null)
            {
                var discordUserWhoIs = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID).GetUser(discordUser.Id);

                string getUsername = null;

                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => discordUser.Id == x.DiscordUserId).FirstOrDefault();

                    if (user != null)
                    {
                        getUsername = user.Username;
                    }
                }

                if (getUsername == null)
                {
                    embed.AddField("Twitch name: ", "N/A", true);
                    embed.AddField("Discord join date: ", discordUserWhoIs.JoinedAt.Value.DateTime.ToString());
                    embed.AddField("Discord account creation date: ", discordUserWhoIs.CreatedAt.DateTime.ToString());
                    return embed;
                }

                using (var context = new DatabaseContext())
                {
                    var user = context.Users.Where(x => discordUser.Id == x.DiscordUserId).FirstOrDefault();

                    embed.AddField("Twitch name: ", getUsername);
                    embed.AddField("Discord join date: ", discordUserWhoIs.JoinedAt.Value.DateTime.ToString());
                    embed.AddField("Discord account creation date: ", discordUserWhoIs.CreatedAt.DateTime.ToString());
                    embed.AddField("Last time seen in stream: ", user.LastSeenDate.ToString());
                    embed.AddField("Total messages sent: ", user.TotalMessages.ToString("N0"));
                    return embed;
                }
            }

            if (twitchUsername == null)
            {
                return null;
            }

            ulong discordId = 0;

            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => twitchUsername == x.Username).FirstOrDefault();

                if (user != null)
                {
                    discordId = user.DiscordUserId;
                }
            }

            if (discordId == 0)
            {
                var list = new List<string>
                {
                    "yoan",
                    "ben dover",
                    "eileen dover",
                    "deborahs wind turbine",
                    "mod mark",
                    "bas",
                    "strieb",
                    "Biggus Dikkus",
                    "not actually strieb",
                    "the sheep",
                    "a rank beggar",
                    "🌲🌲🌲",
                    "https://www.youtube.com/watch?v=6n3pFFPSlW4",
                    "https://www.youtube.com/watch?v=GIDG-Ki7jrc"
                };
                var random = new Random();
                embed.AddField("Twitch name:", twitchUsername.ToLower(), true);
                embed.AddField("Discord name:", list[random.Next(0, list.Count + 1)], true);
                return embed;
            }

            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => discordUser.Id == x.DiscordUserId).FirstOrDefault();

                embed.AddField("Twitch name:", twitchUsername.ToLower(), true);
                embed.AddField("Discord name:", $"<@{discordId}>", true);
                embed.AddField("Last time seen in stream: ", user.LastSeenDate.ToString());
                embed.AddField("Total messages sent: ", user.TotalMessages.ToString("N0"));
                return embed;
            }
        }
    }
}