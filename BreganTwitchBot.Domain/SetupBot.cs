using BreganTwitchBot.Domain.Data.DiscordBot;

namespace BreganTwitchBot.Domain
{
    public class SetupBot
    {
        public static async Task Setup()
        {
            #region Discord

            await Task.Delay(2000);

            //Start Discord
            var discordThread = new Thread(DiscordConnection.MainAsync().GetAwaiter().GetResult);
            discordThread.Start();

            #endregion Discord


        }
    }
}
