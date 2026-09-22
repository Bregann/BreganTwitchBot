using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.Concurrent;

namespace BreganTwitchBot.Domain.Services.Twitch
{
    /// <summary>
    /// Posts a channel's recurring messages to chat.
    ///
    /// A message only goes out if enough chat has happened since it last did, so a quiet
    /// stream does not end up with the bot talking to itself.
    /// </summary>
    public class TimedMessageService(IServiceProvider serviceProvider, ITwitchHelperService twitchHelperService) : ITimedMessageService
    {
        /// <summary>
        /// Chat message count when each timed message last went out, so the gap since can be
        /// worked out. Keyed by the timed message's id.
        /// </summary>
        private readonly ConcurrentDictionary<int, int> _chatCountAtLastSend = new();

        public async Task SendDueMessagesAsync()
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var messages = await context.ChannelTimedMessages
                .Where(x => x.Enabled)
                .Include(x => x.Channel)
                .ToListAsync();

            foreach (var message in messages)
            {
                try
                {
                    await TrySendAsync(context, message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"[Timed Messages] Error sending message {message.Id} in {message.Channel.BroadcasterTwitchChannelName}");
                }
            }

            await context.SaveChangesAsync();
        }

        private async Task TrySendAsync(AppDbContext context, Database.Models.ChannelTimedMessage message)
        {
            var broadcasterId = message.Channel.BroadcasterTwitchChannelId;

            if (message.OnlyWhenLive && !await twitchHelperService.IsBroadcasterLive(broadcasterId))
            {
                return;
            }

            // not long enough since it last went out
            if (message.LastSentAt != null && DateTime.UtcNow - message.LastSentAt.Value < TimeSpan.FromMinutes(message.IntervalMinutes))
            {
                return;
            }

            var chatCount = twitchHelperService.GetChatMessageCount(broadcasterId);

            // the gate only applies once the message has been sent at least once. Applying it
            // to the first send would hold a message back until chat had built up volume it
            // never had the chance to, which reads as the feature being broken
            if (message.MinimumChatMessages > 0 && _chatCountAtLastSend.TryGetValue(message.Id, out var countAtLastSend))
            {
                // the running count resets when a stream ends, so treat a count that has gone
                // backwards as a fresh stream rather than holding the message forever
                var messagesSince = chatCount >= countAtLastSend ? chatCount - countAtLastSend : chatCount;

                if (messagesSince < message.MinimumChatMessages)
                {
                    return;
                }
            }

            await twitchHelperService.SendTwitchMessageToChannel(broadcasterId, message.Channel.BroadcasterTwitchChannelName, message.Message);

            message.LastSentAt = DateTime.UtcNow;
            _chatCountAtLastSend[message.Id] = chatCount;

            Log.Information($"[Timed Messages] Sent message {message.Id} in {message.Channel.BroadcasterTwitchChannelName}");

            // the context is saved once by the caller after every message has been considered
            await Task.CompletedTask;
        }
    }
}
