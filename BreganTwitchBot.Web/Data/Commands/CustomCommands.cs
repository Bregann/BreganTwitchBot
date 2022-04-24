namespace BreganTwitchBot.Web.Data.Commands
{
    public class CustomCommands
    {
        public string CommandName { get; set; }
        public string CommandText { get; set; }
        public DateTime LastUsed { get; set; }
        public long TimesUsed { get; set; }
    }
}
