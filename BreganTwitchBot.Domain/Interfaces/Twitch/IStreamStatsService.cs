using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Interfaces.Twitch
{
    public interface IStreamStatsService
    {
        /// <summary>
        /// Adds to a counter for the channel's current stream. Counts are held in memory and
        /// flushed to the database periodically, so this is cheap enough to call per message.
        /// Does nothing when the channel isn't live.
        /// </summary>
        void UpdateStreamStat(string broadcasterChannelId, StreamStatType statType, long amount = 1);

        /// <summary>
        /// Records a viewer as having been seen in the current stream
        /// </summary>
        void AddUniqueViewer(string broadcasterChannelId, string username);

        /// <summary>
        /// Writes the in memory counters to the database and clears them
        /// </summary>
        Task FlushStats();

        /// <summary>
        /// Starts a new stream row for a channel, capturing the opening follower and sub counts
        /// </summary>
        Task StartNewStream(string broadcasterChannelId);

        /// <summary>
        /// Closes off the channel's current stream, capturing the closing counts and uptime
        /// </summary>
        Task EndStream(string broadcasterChannelId);

        /// <summary>
        /// Records the current viewer count for the channel, for the average and peak
        /// </summary>
        Task RecordViewerCount(string broadcasterChannelId, int viewerCount);
    }
}
