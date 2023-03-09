using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.DiscordBot.Events
{
    public class VoiceStateUpdatedEvent
    {
        public static async Task AddOrRemoveVCRole(SocketUser socketUser, SocketVoiceState voiceState)
        {
            var guild = DiscordConnection.DiscordClient.GetGuild(AppConfig.DiscordGuildID);
            var user = guild.GetUser(socketUser.Id);
            var vcRole = guild.Roles.First(x => x.Name == "VC");

            if (voiceState.VoiceChannel == null)
            {
                await user.RemoveRoleAsync(vcRole);
            }
            else
            {
                await user.AddRoleAsync(vcRole);
            }
        }
    }
}
