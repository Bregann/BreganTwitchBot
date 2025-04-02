using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.EntityFrameworkCore;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands.Linking
{
    public class LinkingDataService(AppDbContext context) : ILinkingDataService
    {
        public async Task<string?> HandleLinkCommand(ChannelChatMessageReceivedParams msgParams)
        {
            var channel = await context.Channels.FirstAsync(x => x.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId);
            
            if (!channel.DiscordEnabled)
            {
                return null;
            }

            // grab the latest request
            var linkReq = await context.DiscordLinkRequests.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.TwitchUserId == msgParams.ChatterChannelId && x.ChannelId == channel.Id);

            if (linkReq == null)
            {
                return "You have not requested to link your account yet. Please use /link in the discord server to link your account.";
            }

            var user = await context.ChannelUsers.FirstAsync(x => x.TwitchUserId == msgParams.ChatterChannelId);
            user.DiscordUserId = linkReq.DiscordUserId;

            await context.SaveChangesAsync();

            return "Your Twitch and Discord have been linked!";
        }
    }
}
