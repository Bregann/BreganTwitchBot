using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.Levelling;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Infrastructure.Database.Enums;
using Discord;
using Discord.WebSocket;
using Hangfire;
using Serilog;
using System.Text.RegularExpressions;

namespace BreganTwitchBot.Domain.Data.DiscordBot.Events
{
    public class MessageReceivedEvent
    {
        private static List<string> _permBanWords = new();
        private static readonly List<string> _urlDomains = new() { ".xxx", ".tv", ".travel", ".tel", ".ru", ".org", ".net", ".name", ".museum", ".mobi", ".me", ".ly", ".jobs", ".int", ".info", ".gov", ".gg", ".coop", ".xyz", ".com", ".co.uk", ".cat", ".biz", "asia", ".aero", ".gift" };
        private static long _lastAskedMessageCount = 6;
        public static Dictionary<ulong, DateTime> XpCooldownDict = new Dictionary<ulong, DateTime>();

        public static void LoadBlacklistedWords()
        {
            using (var context = new DatabaseContext())
            {
                _permBanWords = context.Blacklist.Where(x => x.WordType == WordTypes.PermBanWord).Select(x => x.Word).ToList();
            }
        }

        public static async Task CheckBlacklistedWords(SocketMessage message)
        {
            _lastAskedMessageCount++;

            var removedMessage = message.Content.Replace(Environment.NewLine, "").Replace(" ", "");
            if (removedMessage == "****" || removedMessage == "__")
            {
                await message.DeleteAsync();
                return;
            }

            if (message.Author.IsBot)
            {
                return;
            }

            if (message.Channel.Id == AppConfig.DiscordSocksChannelID)
            {
                await message.DeleteAsync();
            }

            if (message.Content.ToLower().Contains("nitro") || message.Content.ToLower().Contains(".gift"))
            {
                Log.Information("[Discord Scam Check] Potential scam");

                if (message.Content.ToLower().Contains("free") || message.Content.ToLower().Contains("gift") || message.Content.ToLower().Contains("nitro") || message.Content.ToLower().Contains(".gift"))
                {
                    if (message.Content.ToLower().Contains("discord.com") || message.Content.ToLower().Contains("discord.gg"))
                    {
                        Log.Information("[Discord Scam Check] Not a scam");
                    }
                    else
                    {
                        if (_urlDomains.Any(message.Content.ToLower().Contains))
                        {
                            Log.Information("[Discord Scam Check] probably a scam");
                            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
                            var user = guild.GetUser(message.Author.Id);
                            var muteRole = guild.Roles.First(x => x.Name == "mute");

                            await user.AddRoleAsync(muteRole);
                            await DiscordHelper.SendMessage(message.Channel.Id, $"<@&{AppConfig.DiscordBanRole}> potential scam link");
                            return;
                        }
                    }
                }
            }

            var messageRegex = Regex.Replace(message.Content, @"[^0-9a-zA-Z⠁⠂⠃⠄⠅⠆⠇⠈⠉⠊⠋⠌⠍⠎⠏⠐⠑⠒⠓⠔⠕⠖⠗⠘⠙⠚⠛⠜⠝⠞⠟⠠⠡⠢⠣⠤⠥⠦⠧⠨⠩⠪⠫⠬⠭⠮⠯⠰⠱⠲⠳⠴⠵⠶⠷⠸⠹⠺⠻⠼⠽⠾⠿⡀⡁⡂⡃⡄⡅⡆⡇⡈⡉⡊⡋⡌⡍⡎⡏⡐⡑⡒⡓⡔⡕⡖⡗⡘⡙⡚⡛⡜⡝⡞⡟⡠⡡⡢⡣⡤⡥⡦⡧⡨⡩⡪⡫⡬⡭⡮⡯⡰⡱⡲⡳⡴⡵⡶⡷⡸⡹⡺⡻⡼⡽⡾⡿⢀⢁⢂⢃⢄⢅⢆⢇⢈⢉⢊⢋⢌⢍⢎⢏⢐⢑⢒⢓⢔⢕⢖⢗⢘⢙⢚⢛⢜⢝⢞⢟⢠⢡⢢⢣⢤⢥⢦⢧⢨⢩⢪⢫⢬⢭⢮⢯⢰⢱⢲⢳⢴⢵⢶⢷⢸⢹⢺⢻⢼⢽⢾⢿⣀⣁⣂⣃⣄⣅⣆⣇⣈⣉⣊⣋⣌⣍⣎⣏⣐⣑⣒⣓⣔⣕⣖⣗⣘⣙⣚⣛⣜⣝⣞⣟⣠⣡⣢⣣⣤⣥⣦⣧⣨⣩⣪⣫⣬⣭⣮⣯⣰⣱⣲⣳⣴⣵⣶⣷⣸⣹⣺⣻⣼⣽⣾⣿░█▄▌▀─\p{L}]+", "").ToLower();

            //Racist children
            if (_permBanWords.Any(messageRegex.Contains))
            {
                var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
                var user = guild.GetUser(message.Author.Id);
                var muteRole = guild.Roles.First(x => x.Name == "mute");

                await user.AddRoleAsync(muteRole);
                await DiscordHelper.SendMessage(message.Channel.Id, $"<@&{AppConfig.DiscordBanRole}> potential racist edgelord child");
                return;
            }
        }

        public static async Task HandleCustomCommand(SocketMessage message)
        {
            var isMod = DiscordHelper.IsUserMod(message.Author.Id);
            var commandName = message.Content.Split(" ")[0].ToLower();

            //Check if its actually a command
            if (!commandName.StartsWith('!') || message.Author.IsBot)
            {
                return;
            }

            if (message.Channel.Id != AppConfig.DiscordCommandsChannelID && !isMod)
            {
                return;
            }

            if (!CommandHandler.Commands.Contains(commandName))
            {
                return;
            }

            using (var context = new DatabaseContext())
            {
                var command = context.Commands.First(x => x.CommandName == commandName);


                if (command.CommandText.Contains("[user]") || command.CommandText.Contains("[count]"))
                {
                    return;
                }

                await DiscordHelper.SendMessage(message.Channel.Id, command.CommandText);
            }
        }
    }
}
