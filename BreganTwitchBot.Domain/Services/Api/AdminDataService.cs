using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.DTOs.Api;
using BreganTwitchBot.Domain.Interfaces.Api;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Api
{
    /// <summary>
    /// Writes for the admin area.
    ///
    /// Permission checks happen in the filter on the endpoint, so these methods assume
    /// the caller is allowed. They still validate the data itself, since a permitted
    /// user can still send nonsense.
    /// </summary>
    public class AdminDataService(AppDbContext context, ICommandHandler commandHandler) : IAdminDataService
    {
        public async Task<GetChannelConfigResponse?> GetChannelConfigAsync(string broadcasterChannelName)
        {
            var channel = await GetChannel(broadcasterChannelName);

            if (channel == null)
            {
                return null;
            }

            return new GetChannelConfigResponse
            {
                ChannelCurrencyName = channel.ChannelConfig.ChannelCurrencyName,
                CurrencyPointCap = channel.ChannelConfig.CurrencyPointCap,
                DiscordEnabled = channel.ChannelConfig.DiscordEnabled
            };
        }

        public async Task UpdateChannelConfigAsync(string broadcasterChannelName, UpdateChannelConfigRequest request)
        {
            var channel = await GetChannelOrThrow(broadcasterChannelName);

            if (string.IsNullOrWhiteSpace(request.ChannelCurrencyName))
            {
                throw new ArgumentException("The currency name cannot be empty");
            }

            if (request.CurrencyPointCap <= 0)
            {
                throw new ArgumentException("The point cap must be greater than zero");
            }

            channel.ChannelConfig.ChannelCurrencyName = request.ChannelCurrencyName.Trim();
            channel.ChannelConfig.CurrencyPointCap = request.CurrencyPointCap;

            await context.SaveChangesAsync();
            Log.Information($"[Admin] Channel config updated for {broadcasterChannelName}");
        }

        public async Task UpsertCustomCommandAsync(string broadcasterChannelName, UpsertCustomCommandRequest request)
        {
            var channel = await GetChannelOrThrow(broadcasterChannelName);

            var commandName = request.CommandName.Trim().ToLower();

            if (!commandName.StartsWith('!'))
            {
                commandName = "!" + commandName;
            }

            if (commandName.Length < 2)
            {
                throw new ArgumentException("The command name cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(request.CommandText))
            {
                throw new ArgumentException("The command response cannot be empty");
            }

            // a custom command that shadows a built in one would never fire, as the
            // built in handler runs first
            if (commandHandler.IsSystemCommand(commandName))
            {
                throw new ArgumentException($"{commandName} is a built in command and cannot be overridden");
            }

            var existing = await context.CustomCommands
                .FirstOrDefaultAsync(x => x.ChannelId == channel.Id && x.CommandName == commandName);

            if (existing == null)
            {
                context.CustomCommands.Add(new CustomCommand
                {
                    ChannelId = channel.Id,
                    CommandName = commandName,
                    CommandText = request.CommandText.Trim(),
                    LastUsed = DateTime.UtcNow,
                    TimesUsed = 0
                });

                // the handler caches the command list, so it has to be told about new ones
                commandHandler.AddCustomCommand(commandName, channel.BroadcasterTwitchChannelId);
            }
            else
            {
                existing.CommandText = request.CommandText.Trim();
            }

            await context.SaveChangesAsync();
            Log.Information($"[Admin] Custom command {commandName} saved for {broadcasterChannelName}");
        }

        public async Task DeleteCustomCommandAsync(string broadcasterChannelName, string commandName)
        {
            var channel = await GetChannelOrThrow(broadcasterChannelName);
            var normalised = commandName.Trim().ToLower();

            var command = await context.CustomCommands
                .FirstOrDefaultAsync(x => x.ChannelId == channel.Id && x.CommandName == normalised);

            if (command == null)
            {
                throw new KeyNotFoundException($"{commandName} is not a command in this channel");
            }

            context.CustomCommands.Remove(command);
            commandHandler.RemoveCustomCommand(normalised, channel.BroadcasterTwitchChannelId);

            await context.SaveChangesAsync();
            Log.Information($"[Admin] Custom command {normalised} deleted from {broadcasterChannelName}");
        }

        public async Task<List<GetChannelRankResponse>?> GetRanksAsync(string broadcasterChannelName)
        {
            var channel = await GetChannel(broadcasterChannelName);

            if (channel == null)
            {
                return null;
            }

            return await context.ChannelRanks
                .Where(x => x.ChannelId == channel.Id)
                .OrderBy(x => x.RankMinutesRequired)
                .Select(x => new GetChannelRankResponse
                {
                    Id = x.Id,
                    RankName = x.RankName,
                    RankMinutesRequired = x.RankMinutesRequired,
                    BonusRankPointsEarned = x.BonusRankPointsEarned,
                    DiscordRoleId = x.DiscordRoleId
                })
                .ToListAsync();
        }

        public async Task UpsertRankAsync(string broadcasterChannelName, UpsertChannelRankRequest request)
        {
            var channel = await GetChannelOrThrow(broadcasterChannelName);

            if (string.IsNullOrWhiteSpace(request.RankName))
            {
                throw new ArgumentException("The rank name cannot be empty");
            }

            if (request.RankMinutesRequired < 0)
            {
                throw new ArgumentException("The minutes required cannot be negative");
            }

            if (request.Id == null)
            {
                context.ChannelRanks.Add(new ChannelRank
                {
                    ChannelId = channel.Id,
                    RankName = request.RankName.Trim(),
                    RankMinutesRequired = request.RankMinutesRequired,
                    BonusRankPointsEarned = request.BonusRankPointsEarned,
                    DiscordRoleId = request.DiscordRoleId
                });
            }
            else
            {
                var rank = await context.ChannelRanks
                    .FirstOrDefaultAsync(x => x.Id == request.Id && x.ChannelId == channel.Id);

                if (rank == null)
                {
                    throw new KeyNotFoundException("That rank does not exist in this channel");
                }

                rank.RankName = request.RankName.Trim();
                rank.RankMinutesRequired = request.RankMinutesRequired;
                rank.BonusRankPointsEarned = request.BonusRankPointsEarned;
                rank.DiscordRoleId = request.DiscordRoleId;
            }

            await context.SaveChangesAsync();
            Log.Information($"[Admin] Rank {request.RankName} saved for {broadcasterChannelName}");
        }

        public async Task DeleteRankAsync(string broadcasterChannelName, int rankId)
        {
            var channel = await GetChannelOrThrow(broadcasterChannelName);

            var rank = await context.ChannelRanks.FirstOrDefaultAsync(x => x.Id == rankId && x.ChannelId == channel.Id);

            if (rank == null)
            {
                throw new KeyNotFoundException("That rank does not exist in this channel");
            }

            // the progress rows point at this rank, so they go first
            var progress = context.ChannelUserRankProgress.Where(x => x.ChannelRankId == rankId);
            context.ChannelUserRankProgress.RemoveRange(progress);
            context.ChannelRanks.Remove(rank);

            await context.SaveChangesAsync();
            Log.Information($"[Admin] Rank {rank.RankName} deleted from {broadcasterChannelName}");
        }

        public async Task<List<GetBlacklistWordResponse>?> GetBlacklistAsync(string broadcasterChannelName)
        {
            var channel = await GetChannel(broadcasterChannelName);

            if (channel == null)
            {
                return null;
            }

            return await context.Blacklist
                .Where(x => x.ChannelId == channel.Id)
                .OrderBy(x => x.WordType)
                .ThenBy(x => x.Word)
                .Select(x => new GetBlacklistWordResponse
                {
                    Id = x.Id,
                    Word = x.Word,
                    WordType = x.WordType
                })
                .ToListAsync();
        }

        public async Task AddBlacklistWordAsync(string broadcasterChannelName, AddBlacklistWordRequest request)
        {
            var channel = await GetChannelOrThrow(broadcasterChannelName);
            var word = request.Word.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(word))
            {
                throw new ArgumentException("The word cannot be empty");
            }

            if (await context.Blacklist.AnyAsync(x => x.ChannelId == channel.Id && x.Word == word))
            {
                throw new ArgumentException($"{word} is already on the blacklist");
            }

            context.Blacklist.Add(new Blacklist
            {
                ChannelId = channel.Id,
                Word = word,
                WordType = request.WordType
            });

            await context.SaveChangesAsync();
            Log.Information($"[Admin] Blacklist word added to {broadcasterChannelName}");
        }

        public async Task DeleteBlacklistWordAsync(string broadcasterChannelName, int wordId)
        {
            var channel = await GetChannelOrThrow(broadcasterChannelName);

            var word = await context.Blacklist.FirstOrDefaultAsync(x => x.Id == wordId && x.ChannelId == channel.Id);

            if (word == null)
            {
                throw new KeyNotFoundException("That word is not on this channel's blacklist");
            }

            context.Blacklist.Remove(word);
            await context.SaveChangesAsync();
        }

        public async Task<GetDiscordConfigResponse?> GetDiscordConfigAsync(string broadcasterChannelName)
        {
            var channel = await GetChannel(broadcasterChannelName);

            if (channel == null)
            {
                return null;
            }

            var config = channel.ChannelConfig;

            return new GetDiscordConfigResponse
            {
                DiscordEnabled = config.DiscordEnabled,
                DiscordGuildId = config.DiscordGuildId,
                DiscordEventChannelId = config.DiscordEventChannelId,
                DiscordStreamAnnouncementChannelId = config.DiscordStreamAnnouncementChannelId,
                DiscordUserCommandsChannelId = config.DiscordUserCommandsChannelId,
                DiscordUserRankUpAnnouncementChannelId = config.DiscordUserRankUpAnnouncementChannelId,
                DiscordGiveawayChannelId = config.DiscordGiveawayChannelId,
                DiscordGeneralChannelId = config.DiscordGeneralChannelId,
                DiscordModeratorRoleId = config.DiscordModeratorRoleId,
                DiscordWelcomeMessageChannelId = config.DiscordWelcomeMessageChannelId
            };
        }

        public async Task UpdateDiscordConfigAsync(string broadcasterChannelName, UpdateDiscordConfigRequest request)
        {
            var channel = await GetChannelOrThrow(broadcasterChannelName);
            var config = channel.ChannelConfig;

            // without a guild there is nothing for the discord features to act on
            if (request.DiscordEnabled && request.DiscordGuildId == null)
            {
                throw new ArgumentException("A Discord guild id is needed to enable Discord");
            }

            config.DiscordEnabled = request.DiscordEnabled;
            config.DiscordGuildId = request.DiscordGuildId;
            config.DiscordEventChannelId = request.DiscordEventChannelId;
            config.DiscordStreamAnnouncementChannelId = request.DiscordStreamAnnouncementChannelId;
            config.DiscordUserCommandsChannelId = request.DiscordUserCommandsChannelId;
            config.DiscordUserRankUpAnnouncementChannelId = request.DiscordUserRankUpAnnouncementChannelId;
            config.DiscordGiveawayChannelId = request.DiscordGiveawayChannelId;
            config.DiscordGeneralChannelId = request.DiscordGeneralChannelId;
            config.DiscordModeratorRoleId = request.DiscordModeratorRoleId;
            config.DiscordWelcomeMessageChannelId = request.DiscordWelcomeMessageChannelId;

            await context.SaveChangesAsync();
            Log.Information($"[Admin] Discord config updated for {broadcasterChannelName}");
        }

        // Channel point rewards

        public async Task<List<GetChannelPointRewardResponse>?> GetRewardsAsync(string broadcasterChannelName)
        {
            var channel = await GetChannel(broadcasterChannelName);

            if (channel == null)
            {
                return null;
            }

            return await context.ChannelPointRewards
                .Where(x => x.ChannelId == channel.Id)
                .OrderBy(x => x.RewardTitle)
                .Select(x => new GetChannelPointRewardResponse
                {
                    Id = x.Id,
                    RewardTitle = x.RewardTitle,
                    ResponseMessage = x.ResponseMessage,
                    Enabled = x.Enabled,
                    TimesRedeemed = x.TimesRedeemed
                })
                .ToListAsync();
        }

        public async Task UpsertRewardAsync(string broadcasterChannelName, UpsertChannelPointRewardRequest request)
        {
            var channel = await GetChannelOrThrow(broadcasterChannelName);

            if (string.IsNullOrWhiteSpace(request.RewardTitle))
            {
                throw new ArgumentException("The reward title cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(request.ResponseMessage))
            {
                throw new ArgumentException("The response message cannot be empty");
            }

            var title = request.RewardTitle.Trim();

            if (request.Id == null)
            {
                // the title is how a redemption is matched, so two rewards sharing one
                // would make which fires arbitrary
                if (await context.ChannelPointRewards.AnyAsync(x => x.ChannelId == channel.Id && x.RewardTitle.ToLower() == title.ToLower()))
                {
                    throw new ArgumentException($"There is already a reward called {title}");
                }

                context.ChannelPointRewards.Add(new ChannelPointReward
                {
                    ChannelId = channel.Id,
                    RewardTitle = title,
                    ResponseMessage = request.ResponseMessage.Trim(),
                    Enabled = request.Enabled,
                    TimesRedeemed = 0
                });
            }
            else
            {
                var reward = await context.ChannelPointRewards
                    .FirstOrDefaultAsync(x => x.Id == request.Id && x.ChannelId == channel.Id);

                if (reward == null)
                {
                    throw new KeyNotFoundException("That reward does not exist in this channel");
                }

                reward.RewardTitle = title;
                reward.ResponseMessage = request.ResponseMessage.Trim();
                reward.Enabled = request.Enabled;
            }

            await context.SaveChangesAsync();
            Log.Information($"[Admin] Channel point reward {title} saved for {broadcasterChannelName}");
        }

        public async Task DeleteRewardAsync(string broadcasterChannelName, int rewardId)
        {
            var channel = await GetChannelOrThrow(broadcasterChannelName);

            var reward = await context.ChannelPointRewards.FirstOrDefaultAsync(x => x.Id == rewardId && x.ChannelId == channel.Id);

            if (reward == null)
            {
                throw new KeyNotFoundException("That reward does not exist in this channel");
            }

            context.ChannelPointRewards.Remove(reward);
            await context.SaveChangesAsync();
        }

        // Subathon rate bands

        public async Task<List<GetSubathonRateResponse>?> GetSubathonRatesAsync(string broadcasterChannelName)
        {
            var channel = await GetChannel(broadcasterChannelName);

            if (channel == null)
            {
                return null;
            }

            return await context.SubathonRates
                .Where(x => x.ChannelId == channel.Id)
                .OrderBy(x => x.FromHours)
                .Select(x => new GetSubathonRateResponse
                {
                    Id = x.Id,
                    FromHours = x.FromHours,
                    MillisecondsPerBit = x.MillisecondsPerBit,
                    Tier1SubMinutes = x.Tier1SubMinutes,
                    Tier2SubMinutes = x.Tier2SubMinutes,
                    Tier3SubMinutes = x.Tier3SubMinutes
                })
                .ToListAsync();
        }

        public async Task UpsertSubathonRateAsync(string broadcasterChannelName, UpsertSubathonRateRequest request)
        {
            var channel = await GetChannelOrThrow(broadcasterChannelName);

            if (request.FromHours < 0)
            {
                throw new ArgumentException("The hours cannot be negative");
            }

            if (request.MillisecondsPerBit < 0 || request.Tier1SubMinutes < 0 || request.Tier2SubMinutes < 0 || request.Tier3SubMinutes < 0)
            {
                throw new ArgumentException("The rates cannot be negative");
            }

            var duplicate = await context.SubathonRates
                .FirstOrDefaultAsync(x => x.ChannelId == channel.Id && x.FromHours == request.FromHours && x.Id != request.Id);

            // two bands starting at the same hour would make the rate ambiguous
            if (duplicate != null)
            {
                throw new ArgumentException($"There is already a band starting at {request.FromHours} hours");
            }

            if (request.Id == null)
            {
                context.SubathonRates.Add(new SubathonRate
                {
                    ChannelId = channel.Id,
                    FromHours = request.FromHours,
                    MillisecondsPerBit = request.MillisecondsPerBit,
                    Tier1SubMinutes = request.Tier1SubMinutes,
                    Tier2SubMinutes = request.Tier2SubMinutes,
                    Tier3SubMinutes = request.Tier3SubMinutes
                });
            }
            else
            {
                var rate = await context.SubathonRates.FirstOrDefaultAsync(x => x.Id == request.Id && x.ChannelId == channel.Id);

                if (rate == null)
                {
                    throw new KeyNotFoundException("That rate band does not exist in this channel");
                }

                rate.FromHours = request.FromHours;
                rate.MillisecondsPerBit = request.MillisecondsPerBit;
                rate.Tier1SubMinutes = request.Tier1SubMinutes;
                rate.Tier2SubMinutes = request.Tier2SubMinutes;
                rate.Tier3SubMinutes = request.Tier3SubMinutes;
            }

            await context.SaveChangesAsync();
            Log.Information($"[Admin] Subathon rate band from {request.FromHours}h saved for {broadcasterChannelName}");
        }

        public async Task DeleteSubathonRateAsync(string broadcasterChannelName, int rateId)
        {
            var channel = await GetChannelOrThrow(broadcasterChannelName);

            var rate = await context.SubathonRates.FirstOrDefaultAsync(x => x.Id == rateId && x.ChannelId == channel.Id);

            if (rate == null)
            {
                throw new KeyNotFoundException("That rate band does not exist in this channel");
            }

            // with no band covering zero hours a subathon would earn nothing at all
            if (rate.FromHours == 0)
            {
                throw new ArgumentException("The band starting at 0 hours cannot be removed, as nothing would be earned below the next band");
            }

            context.SubathonRates.Remove(rate);
            await context.SaveChangesAsync();
        }

        // Giveaway weighting

        public async Task<GetGiveawayConfigResponse?> GetGiveawayConfigAsync(string broadcasterChannelName)
        {
            var channel = await GetChannel(broadcasterChannelName);

            if (channel == null)
            {
                return null;
            }

            var config = await context.DiscordGiveawayConfigs.FirstOrDefaultAsync(x => x.ChannelId == channel.Id);

            // the defaults the bot falls back to until a channel saves its own
            return new GetGiveawayConfigResponse
            {
                MinutesPerEntry = config?.MinutesPerEntry ?? 3600,
                XpPerEntry = config?.XpPerEntry ?? 1000,
                MaxXpEntries = config?.MaxXpEntries ?? 30,
                RanksGrantEntries = config?.RanksGrantEntries ?? true
            };
        }

        public async Task UpdateGiveawayConfigAsync(string broadcasterChannelName, UpdateGiveawayConfigRequest request)
        {
            var channel = await GetChannelOrThrow(broadcasterChannelName);

            if (request.MinutesPerEntry <= 0)
            {
                throw new ArgumentException("The minutes per entry must be greater than zero");
            }

            if (request.XpPerEntry <= 0)
            {
                throw new ArgumentException("The xp per entry must be greater than zero");
            }

            if (request.MaxXpEntries < 0)
            {
                throw new ArgumentException("The xp entry cap cannot be negative");
            }

            var config = await context.DiscordGiveawayConfigs.FirstOrDefaultAsync(x => x.ChannelId == channel.Id);

            if (config == null)
            {
                config = new DiscordGiveawayConfig
                {
                    ChannelId = channel.Id,
                    MinutesPerEntry = request.MinutesPerEntry,
                    XpPerEntry = request.XpPerEntry,
                    MaxXpEntries = request.MaxXpEntries,
                    RanksGrantEntries = request.RanksGrantEntries
                };

                context.DiscordGiveawayConfigs.Add(config);
            }
            else
            {
                config.MinutesPerEntry = request.MinutesPerEntry;
                config.XpPerEntry = request.XpPerEntry;
                config.MaxXpEntries = request.MaxXpEntries;
                config.RanksGrantEntries = request.RanksGrantEntries;
            }

            await context.SaveChangesAsync();
            Log.Information($"[Admin] Giveaway config updated for {broadcasterChannelName}");
        }

        private async Task<Channel?> GetChannel(string broadcasterChannelName)
        {
            return await context.Channels
                .FirstOrDefaultAsync(x => x.BroadcasterTwitchChannelName.ToLower() == broadcasterChannelName.ToLower());
        }

        private async Task<Channel> GetChannelOrThrow(string broadcasterChannelName)
        {
            return await GetChannel(broadcasterChannelName)
                ?? throw new KeyNotFoundException("Channel not found");
        }
    }
}
