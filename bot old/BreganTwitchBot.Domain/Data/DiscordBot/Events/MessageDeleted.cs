using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using Discord;
using Serilog;

namespace BreganTwitchBot.Domain.Data.DiscordBot.Events
{
    public class MessageDeletedEvent
    {
        public static async Task SendDeletedMessageToEvents(Cacheable<IMessage, ulong> oldMessage, Cacheable<IMessageChannel, ulong> channel)
        {
            await oldMessage.GetOrDownloadAsync();
            if (oldMessage.Value.Content == null || oldMessage.Value.Author.Id == DiscordConnection.DiscordClient.CurrentUser.Id)
            {
                return;
            }

            if (oldMessage.Value.Content.Replace(Environment.NewLine, "").Replace(" ", "") == "****" || oldMessage.Value.Content.Replace(Environment.NewLine, "").Replace(" ", "") == "__")
            {
                return;
            }

            if (oldMessage.Value.Attachments.Count > 0)
            {
                foreach (var attachment in oldMessage.Value.Attachments)
                {
                    await DiscordHelper.SendMessage(AppConfig.DiscordEventChannelID, $"Message Deleted: {oldMessage.Value.Content.Replace("@everyone", "").Replace("@here", "")} \nImage Name: {attachment.Filename} \nSent By: {oldMessage.Value.Author} \nIn Channel: {channel.Value.Name} \nDeleted at: {DateTime.Now} \n Link: {attachment.Url}");
                    Log.Information($"[Discord Message Deleted] Message Deleted: {oldMessage.Value.Content.Replace("@everyone", "").Replace("@here", "")} \nImage Name: {attachment.Filename} \nSent By: {oldMessage.Value.Author} \nIn Channel: {channel.Value.Name} \nDeleted at: {DateTime.Now} \n Link: {attachment.Url}");
                    return;
                }
            }

            var messageEmbed = new EmbedBuilder()
            {
                Title = "Deleted message",
                Timestamp = DateTime.Now,
                Color = new Discord.Color(250, 53, 27)
            };

            messageEmbed.AddField("Message Deleted", oldMessage.Value.Content.Replace("@everyone", "").Replace("@here", ""));
            messageEmbed.AddField("Sent By", oldMessage.Value.Author.Username);
            messageEmbed.AddField("In Channel", oldMessage.Value.Channel.Name);
            messageEmbed.AddField("Deleted At", DateTime.Now.ToString());

            var eventsChannel = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordEventChannelID) as IMessageChannel;

            if (eventsChannel != null)
            {
                await eventsChannel.TriggerTypingAsync();
                await eventsChannel.SendMessageAsync(embed: messageEmbed.Build());
            }
        }
    }
}
