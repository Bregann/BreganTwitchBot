using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.DTOs.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Services.Discord.SlashCommands.Linking
{
    //TODO: test method
    public class DiscordLinkingData(AppDbContext context) : IDiscordLinkingData
    {
        public async Task<string> NewLinkRequest(DiscordCommand command)
        {
            if (command.CommandText == null)
            {
                return "Please provide a Twitch username.";
            }

            var twitchUsername = command.CommandText.Trim().ToLower();

            var user = await context.ChannelUsers
                .FirstOrDefaultAsync(x => x.TwitchUsername == twitchUsername);

            if (user == null)
            {
                return $"No user found with the Twitch username supplied";
            }

            if (user.DiscordUserId != 0)
            {
                return $"This Twitch user is already linked to a Discord account.";
            }

            var existingRequest = await context.DiscordLinkRequests
                .FirstOrDefaultAsync(x => x.TwitchUsername == twitchUsername);

            var channel = await context.Channels
                .FirstAsync(x => x.DiscordGuildId == command.GuildId);

            if (existingRequest != null)
            {
                return $"A link request already exists for this Twitch username. Please type ```!link {existingRequest.TwitchLinkCode}``` in https://twitch.tv/{channel.BroadcasterTwitchChannelName} to link your account";
            }

            // create the link request
            var linkRequest = new DiscordLinkRequests
            {
                TwitchUsername = twitchUsername,
                DiscordUserId = command.UserId,
                TwitchLinkCode = new Random().Next(0, 999999)
            };

            await context.DiscordLinkRequests.AddAsync(linkRequest);
            await context.SaveChangesAsync();

            return $"A link request has been created for the Twitch username {twitchUsername}. Please type ```!link {linkRequest.TwitchLinkCode}``` in https://twitch.tv/{channel.BroadcasterTwitchChannelName} to link your account";
        }

    }
}
