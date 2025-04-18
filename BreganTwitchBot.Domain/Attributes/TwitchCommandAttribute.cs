namespace BreganTwitchBot.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TwitchCommandAttribute : Attribute
    {
        public string CommandName { get; }
        public string[]? CommandAlias { get; }

        public TwitchCommandAttribute(string commandName, string[]? commandAlias = null)
        {
            CommandName = commandName.ToLower();
            CommandAlias = commandAlias;
        }
    }
}
