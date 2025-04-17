using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Linking
{
    public class LinkingDataService(AppDbContext context, IDiscordHelperService discordHelperService, IDiscordLinkingData discordLinkingData) : ILinkingDataService
    {
        public async Task<string?> HandleLinkCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId);
            
            if (!channel.DiscordEnabled)
            {
                return null;
            }

            // check if there is a link code
            if (msgParams.MessageParts.Length == 1)
            {
                return "You have not requested to link your account yet. Please use /link in the discord server to link your account.";
            }

            // try and pass the link code, should be an int
            if (!int.TryParse(msgParams.MessageParts[1], out var linkCode))
            {
                return "The link code is not valid. Please double check the code and try again";
            }

            // grab the latest request
            var linkReq = await context.DiscordLinkRequests.FirstOrDefaultAsync(x => x.TwitchUsername == msgParams.ChatterChannelName);

            if (linkReq == null)
            {
                return "You have not requested to link your account yet. Please use /link in the discord server to link your account.";
            }

            if (linkReq.TwitchLinkCode != linkCode)
            {
                return "The link code is not valid. Please double check the code and try again";
            }

            var user = await context.ChannelUsers.FirstAsync(x => x.TwitchUserId == msgParams.ChatterChannelId);
            user.DiscordUserId = linkReq.DiscordUserId;
            await context.SaveChangesAsync();

            await discordLinkingData.AddRolesToUserOnLink(msgParams.ChatterChannelId);
            await discordHelperService.AddDiscordUserToDatabaseFromTwitch(msgParams.BroadcasterChannelId, msgParams.ChatterChannelId);

            return "Your Twitch and Discord have been linked!";
        }
    }
}
