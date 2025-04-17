using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.GeneralCommands
{
    public class GeneralCommandModule(IDiscordClientProvider discordClient, IGeneralCommandsData generalCommandsData) : InteractionModuleBase<SocketInteractionContext>
    {
        // for the most part all the logic is in their own commands and not in the seperate DI service as it's mostly Discord edting stuff
        [SlashCommand("botuptime", "See how long the bot is up for")]
        public async Task BotUptime()
        {
            var botUptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            var uptimeString = string.Format("{0:D2}:{1:D2}:{2:D2}", botUptime.Hours, botUptime.Minutes, botUptime.Seconds);
            await RespondAsync($"The bot has been up for {uptimeString}");
        }

        [SlashCommand("christmas", "get yourself some Christmas emojis for your name")]
        public async Task ChristmasName()
        {
            var builder = new ComponentBuilder()
                .WithButton(" ", "christmas-snowman", ButtonStyle.Danger, new Emoji("⛄"))
                .WithButton(" ", "christmas-gift", ButtonStyle.Success, new Emoji("🎁"))
                .WithButton(" ", "christmas-tree", ButtonStyle.Danger, new Emoji("🎄"))
                .WithButton(" ", "christmas-santa", ButtonStyle.Success, new Emoji("🎅"))
                .WithButton(" ", "christmas-mrsanta", ButtonStyle.Danger, new Emoji("🤶"))
                .WithButton(" ", "christmas-star", ButtonStyle.Success, new Emoji("🌟"))
                .WithButton(" ", "christmas-socks", ButtonStyle.Danger, new Emoji("🧦"))
                .WithButton(" ", "christmas-bell", ButtonStyle.Success, new Emoji("🔔"))
                .WithButton(" ", "christmas-deer", ButtonStyle.Danger, new Emoji("🦌"))
                .WithButton(" ", "christmas-resetusername", ButtonStyle.Success, new Emoji("♻️"));

            await RespondAsync("Click the buttons to add a christmas emoji onto your name!", components: builder.Build());
        }

        [SlashCommand("spook", "Give yourself a pumpkin AND spooky ghost")]
        public async Task SpookyName()
        {
            if (DateTime.UtcNow.Month != 10)
            {
                await RespondAsync("You silly pumpkin it's not October!");
            }

            var guild = discordClient.Client.GetGuild(Context.Guild.Id);
            var user = guild.GetUser(Context.User.Id);

            string nickNameToSet = "";

            if (user.Nickname != null)
            {
                nickNameToSet = "🎃" + user.Nickname + "👻";
            }
            else
            {
                nickNameToSet = "🎃" + user.Username + "👻";
            }

            await user.ModifyAsync(user => user.Nickname = nickNameToSet);
            await RespondAsync("Your nickname has been set!");
        }

        [SlashCommand("firework", "Give yourself some fireworks to celebrate fireworks night!")]
        public async Task FireworksName()
        {
            if (DateTime.UtcNow.Month != 11)
            {
                await RespondAsync("You silly firework it's not November!");
            }

            var guild = discordClient.Client.GetGuild(Context.Guild.Id);
            var user = guild.GetUser(Context.User.Id);
            string nickNameToSet = "";

            if (user.Nickname != null)
            {
                nickNameToSet = "🎆" + user.Nickname + "🎇";
            }
            else
            {
                nickNameToSet = "🎆" + user.Username + "🎇";
            }

            await user.ModifyAsync(user => user.Nickname = nickNameToSet);
            await RespondAsync("Your nickname has been set!");
        }

        [SlashCommand("unspook", "remove your spooky name")]
        public async Task UnspookName()
        {
            var guild = discordClient.Client.GetGuild(Context.Guild.Id);
            var user = guild.GetUser(Context.User.Id);
            string nickNameToSet = "";

            if (user.Nickname != null)
            {
                nickNameToSet = user.Nickname.Replace("🎃", "").Replace("👻", "");
                await user.ModifyAsync(user => user.Nickname = nickNameToSet);
            }

            await RespondAsync("You have been unspooked!");
        }

        [SlashCommand("unfirework", "remove your fireworks name")]
        public async Task UnfireworkName()
        {
            var guild = discordClient.Client.GetGuild(Context.Guild.Id);
            var user = guild.GetUser(Context.User.Id);
            string nickNameToSet = "";

            if (user.Nickname != null)
            {
                nickNameToSet = user.Nickname.Replace("🎆", "").Replace("🎇", "");
                await user.ModifyAsync(user => user.Nickname = nickNameToSet);
            }

            await RespondAsync("You have been unfireworked!");
        }

        [SlashCommand("birthday", "[SERVER SPECIFIC!!] Set your birthday to get a Happy Birthday announcement in THE CURRENT SERVER")]
        public async Task Birthday([Summary("day", "the day you were born (number)")] int day, [Summary("month", "the month you were born (number)")] int month)
        {
            var command = new AddBirthdayCommand
            {
                UserId = Context.User.Id,
                GuildId = Context.Guild.Id,
                ChannelId = Context.Channel.Id,
                Day = day,
                Month = month
            };

            var response = await generalCommandsData.AddUserBirthday(command);
            await RespondAsync(response);
        }
    }
}
