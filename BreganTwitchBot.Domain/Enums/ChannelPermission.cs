namespace BreganTwitchBot.Domain.Enums
{
    /// <summary>
    /// What a moderator is allowed to change in a channel's admin area.
    ///
    /// The broadcaster implicitly has all of these. ManagePermissions is deliberately
    /// not grantable - only the broadcaster manages permissions, so a mod cannot
    /// escalate themselves or lock the broadcaster out.
    /// </summary>
    public enum ChannelPermission
    {
        ViewAdmin,
        EditCommands,
        EditRanks,
        EditSubathon,
        EditGiveaways,
        EditBlacklist,
        EditDiscord,
        EditChannelConfig
    }
}
