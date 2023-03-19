using Discord;
using Discord.WebSocket;
using Serilog;

namespace BreganTwitchBot.Domain.Data.DiscordBot.Events
{
    public class PresenceEvent
    {
        public static void TrackUserStatus(SocketUser user, SocketPresence previous, SocketPresence newUserUpdate)
        {
            var previousStatusData = previous.Activities?.Where(x => x.Type == ActivityType.CustomStatus).FirstOrDefault() as CustomStatusGame;
            var newStatusData = newUserUpdate.Activities.Where(x => x.Type == ActivityType.CustomStatus).FirstOrDefault() as CustomStatusGame;


            //If the statuses match then don't process them
            if (previousStatusData?.State == newStatusData?.State)
            {
                return;
            }

            //We might as well log the statuses for the memes
            Log.Information($"[User Status] Discord status for user {user.Username} changed from {previousStatusData?.State ?? "null"} to {newStatusData?.State ?? "null"} - userId {user.Id}");
        }

        public static async Task UpdateStreamerStatusMessage(SocketUser user, SocketPresence previous, SocketPresence newUserUpdate)
        {
            var previousStatusData = previous.Activities?.Where(x => x.Type == ActivityType.CustomStatus).FirstOrDefault() as CustomStatusGame;
            var newStatusData = newUserUpdate.Activities.Where(x => x.Type == ActivityType.CustomStatus).FirstOrDefault() as CustomStatusGame;

            if (user.Id == AppConfig.DiscordGuildOwnerID)
            {
                //Check if the new status data is the same as the known current one
                if (newStatusData != null)
                {
                    //Update it if it's different or if it's a different day to the pinned message, it might be the same as yesterdays
                    if (newStatusData.State != AppConfig.PinnedStreamMessage || AppConfig.PinnedMessageDate.Day != DateTime.UtcNow.Day)
                    {
                        //Make sure its about streaming
                        if (newStatusData.State.ToLower().Contains("stream") || newStatusData.State.ToLower().Contains("live"))
                        {
                            //unpin the old message
                            var channel = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordGeneralChannel) as IMessageChannel;

                            if (AppConfig.PinnedStreamMessageId != 0 && channel != null)
                            {
                                var message = await channel.GetMessageAsync(AppConfig.PinnedStreamMessageId) as IUserMessage;

                                if (message != null)
                                {
                                    await message.UnpinAsync();
                                }
                            }

                            //Send the new message and pin it
                            if (channel != null)
                            {
                                var newMessage = await channel.SendMessageAsync($"New {AppConfig.BroadcasterName} stream update!!!! Update is: {newStatusData.State}");
                                await newMessage.PinAsync();
                                AppConfig.UpdatePinnedMessageAndMessageId(newStatusData.State, newMessage.Id);
                            }
                        }
                    }
                }
            }
        }
    }
}
