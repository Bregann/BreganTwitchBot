using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.OpenAI
{
    public class OpenAIData
    {
        public static async Task<bool> IsAllowedAI(ulong discordUserId)
        {
            using (var context = new DatabaseContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId);
                if (user == null)
                {
                    return false;
                }
                return user.CanUseOpenAi;
            }
        }

        public static async Task AddNewLikedItem(ulong discordUserId, string items, AiType likedItemType)
        {
            using (var context = new DatabaseContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId);
                if (user == null)
                {
                    return;
                }

                var newItems = items.Split(",").Select(x => new AiBookData
                {
                    TwitchUserId = user.TwitchUserId,
                    Value = x,
                    AiType = likedItemType
                });

                await context.AiBookData.AddRangeAsync(newItems);
                await context.SaveChangesAsync();
            }
        }
    }
}
