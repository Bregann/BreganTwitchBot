using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;

namespace BreganTwitchBot.Domain.Services.Helpers
{
    /// <summary>
    /// Reading arguments out of a chat command.
    ///
    /// MessageParts is the raw message split on spaces, so index 0 is always the command itself
    /// and everything after it is the argument. These wrap that indexing up so the commands don't
    /// each repeat the skipping and trimming.
    /// </summary>
    public static class CommandArgumentHelper
    {
        /// <summary>
        /// Everything after the command itself, or null when the command was used on its own.
        /// Whitespace only arguments count as no argument, so "!title    " reads the title
        /// rather than blanking it.
        /// </summary>
        public static string? GetArgument(this ChannelChatMessageReceivedParams msgParams)
        {
            return msgParams.GetArgument(1);
        }

        /// <summary>
        /// Everything from the given part onwards, or null when there is nothing there.
        /// Used by the commands that take a fixed argument first and free text after it,
        /// such as "!addcmd &lt;name&gt; &lt;the command text&gt;"
        /// </summary>
        public static string? GetArgument(this ChannelChatMessageReceivedParams msgParams, int fromPart)
        {
            var argument = string.Join(' ', msgParams.MessageParts.Skip(fromPart)).Trim();
            return string.IsNullOrWhiteSpace(argument) ? null : argument;
        }

        /// <summary>
        /// A single part of the message, or null when it was not supplied. Usernames are passed
        /// around without the leading @ and lowercased, which is how they are stored.
        /// </summary>
        public static string? GetUsernameArgument(this ChannelChatMessageReceivedParams msgParams, int part = 1)
        {
            if (msgParams.MessageParts.Length <= part)
            {
                return null;
            }

            var username = msgParams.MessageParts[part].TrimStart('@').Trim().ToLower();
            return string.IsNullOrWhiteSpace(username) ? null : username;
        }
    }
}
