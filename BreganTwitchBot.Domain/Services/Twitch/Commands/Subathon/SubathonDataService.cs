using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Exceptions;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using BreganTwitchBot.Domain.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands.Subathon
{
    public class SubathonDataService(AppDbContext dbContext, ITwitchHelperService twitchHelperService) : ISubathonDataService
    {
        public async Task AddBitsTime(string broadcasterChannelId, string chatterChannelId, int bitsAmount)
        {
            var config = await GetConfigIfSubathonActive(broadcasterChannelId);

            if (config == null || bitsAmount <= 0)
            {
                return;
            }

            // The rate depends on how much time has already been banked, and a big cheer can push
            // the subathon through a band boundary. The old bot looped once per bit to handle
            // that, which meant 10k iterations for a 10k cheer, so the time is accumulated a band
            // at a time instead - same result, bounded work.
            var rates = await GetRatesAsync(config.ChannelId);
            var timeToAdd = TimeSpan.Zero;
            var runningTotal = config.SubathonTime;
            var bitsLeft = bitsAmount;

            while (bitsLeft > 0)
            {
                var rate = GetRateForTime(rates, runningTotal);
                var millisecondsPerBit = rate?.MillisecondsPerBit ?? 0;

                if (millisecondsPerBit <= 0)
                {
                    break;
                }

                // how many bits can be spent before the next band starts
                var nextBand = rates.Where(x => x.FromHours > Math.Floor(runningTotal.TotalHours)).OrderBy(x => x.FromHours).FirstOrDefault();

                int bitsThisBand;

                if (nextBand == null)
                {
                    bitsThisBand = bitsLeft;
                }
                else
                {
                    var msUntilNextBand = TimeSpan.FromHours(nextBand.FromHours).TotalMilliseconds - runningTotal.TotalMilliseconds;
                    bitsThisBand = Math.Min(bitsLeft, Math.Max(1, (int)Math.Ceiling(msUntilNextBand / millisecondsPerBit)));
                }

                var bandTime = TimeSpan.FromMilliseconds((double)millisecondsPerBit * bitsThisBand);

                timeToAdd += bandTime;
                runningTotal += bandTime;
                bitsLeft -= bitsThisBand;
            }

            await ApplyContribution(config, chatterChannelId, timeToAdd, bitsDonated: bitsAmount, subsGifted: 0);
            Log.Information($"[Subathon] Added {timeToAdd} for {bitsAmount} bits in {broadcasterChannelId}");
        }

        public async Task AddSubTime(string broadcasterChannelId, string chatterChannelId, SubTierEnum subTier, int subCount = 1)
        {
            var config = await GetConfigIfSubathonActive(broadcasterChannelId);

            if (config == null || subCount <= 0)
            {
                return;
            }

            var rates = await GetRatesAsync(config.ChannelId);
            var timeToAdd = TimeSpan.Zero;
            var runningTotal = config.SubathonTime;

            // sub counts are small, so adding them one at a time keeps band changes exact
            for (var i = 0; i < subCount; i++)
            {
                var rate = GetRateForTime(rates, runningTotal);

                if (rate == null)
                {
                    break;
                }

                var minutes = subTier switch
                {
                    SubTierEnum.Tier2 => rate.Tier2SubMinutes,
                    SubTierEnum.Tier3 => rate.Tier3SubMinutes,
                    _ => rate.Tier1SubMinutes
                };

                var subTime = TimeSpan.FromMinutes(minutes);
                timeToAdd += subTime;
                runningTotal += subTime;
            }

            await ApplyContribution(config, chatterChannelId, timeToAdd, bitsDonated: 0, subsGifted: subCount);
            Log.Information($"[Subathon] Added {timeToAdd} for {subCount} {subTier} sub(s) in {broadcasterChannelId}");
        }

        public async Task<string> AddTimeManually(ChannelChatMessageReceivedParams msgParams)
        {
            await twitchHelperService.EnsureUserHasModeratorPermissions(msgParams.IsMod, msgParams.IsBroadcaster, msgParams.ChatterChannelName, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            if (msgParams.MessageParts.Length < 2 || !int.TryParse(msgParams.MessageParts[1], out var seconds))
            {
                throw new InvalidCommandException("The format is !addsubathontime <seconds>");
            }

            var config = await GetConfigIfSubathonActive(msgParams.BroadcasterChannelId);

            if (config == null)
            {
                return "There isn't a subathon running at the moment";
            }

            config.SubathonTime += TimeSpan.FromSeconds(seconds);
            await dbContext.SaveChangesAsync();

            return $"Added {seconds} seconds. The subathon is now {DurationFormatHelper.Humanise(config.SubathonTime)} long";
        }

        public async Task<string> GetSubathonStatus(ChannelChatMessageReceivedParams msgParams)
        {
            var config = await GetConfigIfSubathonActive(msgParams.BroadcasterChannelId);

            if (config == null)
            {
                return $"@{msgParams.ChatterChannelName} => There isn't a subathon running at the moment";
            }

            var totalTime = DurationFormatHelper.Humanise(config.SubathonTime);

            if (config.SubathonStartTime == null)
            {
                return $"@{msgParams.ChatterChannelName} => The subathon has been extended to a total of {totalTime}!";
            }

            var endTime = config.SubathonStartTime.Value.Add(config.SubathonTime);
            var timeLeft = endTime - DateTime.UtcNow;

            if (timeLeft <= TimeSpan.Zero)
            {
                return $"@{msgParams.ChatterChannelName} => The subathon reached a total of {totalTime} and the time is up!";
            }

            return $"@{msgParams.ChatterChannelName} => The subathon has been extended to a total of {totalTime}! The stream will end in {DurationFormatHelper.Humanise(timeLeft)}";
        }

        public async Task<string> StartSubathon(ChannelChatMessageReceivedParams msgParams)
        {
            await twitchHelperService.EnsureUserHasModeratorPermissions(msgParams.IsMod, msgParams.IsBroadcaster, msgParams.ChatterChannelName, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            var config = await dbContext.ChannelConfig.FirstOrDefaultAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId);

            if (config == null)
            {
                throw new InvalidCommandException("Could not find the channel configuration");
            }

            if (config.SubathonActive)
            {
                return "There is already a subathon running!";
            }

            // starting hours come from the command if given, otherwise the subathon starts at zero
            var startingTime = TimeSpan.Zero;

            if (msgParams.MessageParts.Length > 1 && double.TryParse(msgParams.MessageParts[1], out var startingHours))
            {
                if (startingHours < 0)
                {
                    throw new InvalidCommandException("The starting hours can't be negative");
                }

                startingTime = TimeSpan.FromHours(startingHours);
            }

            config.SubathonActive = true;
            config.SubathonTime = startingTime;
            config.SubathonStartTime = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            return $"The subathon has started with {DurationFormatHelper.Humanise(startingTime)} on the clock! PogChamp";
        }

        public async Task<string> StopSubathon(ChannelChatMessageReceivedParams msgParams)
        {
            await twitchHelperService.EnsureUserHasModeratorPermissions(msgParams.IsMod, msgParams.IsBroadcaster, msgParams.ChatterChannelName, msgParams.ChatterChannelId, msgParams.BroadcasterChannelId, msgParams.BroadcasterChannelName);

            var config = await dbContext.ChannelConfig.FirstOrDefaultAsync(x => x.Channel.BroadcasterTwitchChannelId == msgParams.BroadcasterChannelId);

            if (config == null || !config.SubathonActive)
            {
                return "There isn't a subathon running at the moment";
            }

            var finalTime = config.SubathonTime;
            config.SubathonActive = false;
            await dbContext.SaveChangesAsync();

            return $"The subathon has ended after a total of {DurationFormatHelper.Humanise(finalTime)}! <3";
        }

        /// <summary>
        /// Gets the channel config only when a subathon is actually running, so every caller can
        /// no-op cheaply outside of one
        /// </summary>
        private async Task<ChannelConfig?> GetConfigIfSubathonActive(string broadcasterChannelId)
        {
            var config = await dbContext.ChannelConfig.FirstOrDefaultAsync(x => x.Channel.BroadcasterTwitchChannelId == broadcasterChannelId);

            return config is { SubathonActive: true } ? config : null;
        }

        private async Task<List<SubathonRate>> GetRatesAsync(int channelId)
        {
            return await dbContext.SubathonRates
                .Where(x => x.ChannelId == channelId)
                .OrderBy(x => x.FromHours)
                .ToListAsync();
        }

        /// <summary>
        /// The band in force for the given accumulated time - the highest FromHours at or below it
        /// </summary>
        private static SubathonRate? GetRateForTime(List<SubathonRate> rates, TimeSpan currentTime)
        {
            return rates.LastOrDefault(x => x.FromHours <= Math.Floor(currentTime.TotalHours));
        }

        private async Task ApplyContribution(ChannelConfig config, string chatterChannelId, TimeSpan timeToAdd, long bitsDonated, int subsGifted)
        {
            config.SubathonTime += timeToAdd;

            var channelUser = await dbContext.ChannelUsers.FirstOrDefaultAsync(x => x.TwitchUserId == chatterChannelId);

            if (channelUser != null)
            {
                var contribution = await dbContext.Subathons.FirstOrDefaultAsync(x => x.ChannelId == config.ChannelId && x.ChannelUserId == channelUser.Id);

                if (contribution == null)
                {
                    await dbContext.Subathons.AddAsync(new Database.Models.Subathon
                    {
                        ChannelId = config.ChannelId,
                        ChannelUserId = channelUser.Id,
                        BitsDonated = bitsDonated,
                        SubsGifted = subsGifted,
                        TimeAdded = timeToAdd
                    });
                }
                else
                {
                    contribution.BitsDonated += bitsDonated;
                    contribution.SubsGifted += subsGifted;
                    contribution.TimeAdded += timeToAdd;
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
