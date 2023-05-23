using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using Serilog;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Events
{
    public class ChannelPoints
    {
        public static async Task HandleChannelPointsEvent(string rewardTitle, int rewardCost, string userRedeemed, string redemptionStatus)
        {
            try
            {
                Log.Information($"[ChannelPoints] Name: {rewardTitle} Cost: {rewardCost} User who redeemed: {userRedeemed}");
                StreamStatsService.UpdateStreamStat(1, StatTypes.AmountOfRewardsRedeemd);
                StreamStatsService.UpdateStreamStat(rewardCost, StatTypes.RewardRedeemCost);

                if (redemptionStatus == "ACTION_TAKEN")
                {
                    return;
                }

                switch (rewardTitle.ToLower())
                {
                    case "goose":
                        TwitchHelper.SendMessage($"{userRedeemed} has redeemed Goose! Goose Goose Goose Goose Goose Goose Goose Goose Goose Goose Goose Goose Goose Goose Goose");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[ChannelPoints] Error handling channel points event {ex}");
                return;
            }
        }
    }
}
