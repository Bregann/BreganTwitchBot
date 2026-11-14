namespace BreganTwitchBot.Domain.Enums
{
    public enum EnvironmentalSettingEnum
    {
        HangfireUsername,
        HangfirePassword,
        TwitchAPIClientID,
        TwitchAPISecret,
        OpenAiApiKey,
        GeminiApiKey,
        DiscordBotToken,

        // The single bot account used across every channel
        BotTwitchChannelId,
        BotTwitchChannelName,
        BotTwitchChannelOAuthToken,
        BotTwitchChannelRefreshToken,

        // Key for reporting to Project Monitor (copy it from its Manage projects page)
        ProjectMonitorApiKey
    }
}
