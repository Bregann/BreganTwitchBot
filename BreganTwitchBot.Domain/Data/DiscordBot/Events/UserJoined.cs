﻿using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace BreganTwitchBot.Domain.Data.DiscordBot.Events
{
    internal class UserJoinedEvent
    {
        public static async Task SendNewUserInfo(SocketGuildUser userJoined)
        {
            if (userJoined.Guild.Id != AppConfig.DiscordGuildID)
            {
                return;
            }

            await StreamStatsService.UpdateStreamStat(1, StatTypes.AmountOfDiscordUsersJoined);

            var messageEmbed = new EmbedBuilder()
            {
                Title = "User Joined",
                Timestamp = DateTime.Now,
                Color = new Color(3, 207, 252)
            };

            messageEmbed.AddField("Discord Username", userJoined.Username);

            var channel = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordEventChannelID) as IMessageChannel;

            if (channel != null)
            {
                await channel.SendMessageAsync(embed: messageEmbed.Build());
            }

            using (var how2LinkGif = new FileStream($"Skins/how2link.gif", FileMode.Open))
            {
                var channel2 = await DiscordConnection.DiscordClient.GetChannelAsync(AppConfig.DiscordLinkingChannelID) as IMessageChannel;

                if (channel2 != null)
                {
                    await channel2.SendFileAsync(how2LinkGif, "how2link.gif", $"Welcome {userJoined.Mention}! \n\n To access the Discord Server, you must link your Twitch account with Discord by using the ``/link`` command!");
                }
            };
        }
    }
}