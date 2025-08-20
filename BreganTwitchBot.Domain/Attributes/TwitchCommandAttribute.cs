namespace BreganTwitchBot.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TwitchCommandAttribute(string commandName, string[]? commandAlias = null) : Attribute
    {
        public string CommandName { get; } = commandName.ToLower();
        public string[]? CommandAlias { get; } = commandAlias;
    }
}
