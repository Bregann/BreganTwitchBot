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

        // Where Twitch sends a website visitor back to after they sign in
        WebsiteTwitchOAuthRedirectUri
    }
}
