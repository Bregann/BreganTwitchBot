namespace BreganTwitchBot.Domain.DTOs.Helpers
{
    /// <summary>
    /// What the bot has stored about a channel's stream, which is all that survives a restart
    /// </summary>
    public record StreamState(
        bool Live,
        string? TwitchStreamId,
        DateTime LastStreamStart,
        DateTime LastStreamEnd,
        bool DailyPointsAllowed,
        DateTime LastDailyPointsAllowed);
}
