namespace BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents
{
    public class ChannelPointsRedeemedParams : EventBase
    {
        public required string RewardId { get; set; }
        public required string RewardTitle { get; set; }
        public required int RewardCost { get; set; }
        public required string RedemptionStatus { get; set; }

        /// <summary>
        /// Whatever the viewer typed when redeeming, for rewards that take user input
        /// </summary>
        public string? UserInput { get; set; }
    }
}
