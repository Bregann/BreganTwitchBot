﻿using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.Giveaway;
using Discord.WebSocket;

namespace BreganTwitchBot.Domain.Data.DiscordBot.Events
{
    public class ButtonPressedEvent
    {
        public static async Task HandleButtonRoles(SocketMessageComponent arg)
        {
            var addedOrRemoved = true;
            var roleName = "";

            switch (arg.Data.CustomId)
            {
                case "marbolsRole":
                    addedOrRemoved = await DiscordHelper.AddOrRemoveRole("Marbles On Stream", arg.User.Id);
                    roleName = "Marbles On Stream";
                    break;
                case "weebRole":
                    addedOrRemoved = await DiscordHelper.AddOrRemoveRole("Weeb", arg.User.Id);
                    roleName = "Weeb";
                    break;
                case "petsRole":
                    addedOrRemoved = await DiscordHelper.AddOrRemoveRole("Pets", arg.User.Id);
                    roleName = "Pets";
                    break;
                case "programmerRole":
                    addedOrRemoved = await DiscordHelper.AddOrRemoveRole("Programmer", arg.User.Id);
                    roleName = "Programmer";
                    break;
                case "susRole":
                    addedOrRemoved = await DiscordHelper.AddOrRemoveRole("Among Us", arg.User.Id);
                    roleName = "Among Us (sus)";
                    break;
                case "freeGamesRole":
                    addedOrRemoved = await DiscordHelper.AddOrRemoveRole("Free Games", arg.User.Id);
                    roleName = "Free Games";
                    break;
                case "politcsRole":
                    addedOrRemoved = await DiscordHelper.AddOrRemoveRole("Politics", arg.User.Id);
                    roleName = "Politics";
                    break;
                case "photographyRole":
                    addedOrRemoved = await DiscordHelper.AddOrRemoveRole("Photography", arg.User.Id);
                    roleName = "Photography";
                    break;
                case "botUpdatesRole":
                    addedOrRemoved = await DiscordHelper.AddOrRemoveRole("Bot Updates", arg.User.Id);
                    roleName = "Bot Updates";
                    break;
                case "horrorGameRole":
                    addedOrRemoved = await DiscordHelper.AddOrRemoveRole("Horror Game Pings", arg.User.Id);
                    roleName = "Horror Game Pings";
                    break;
                case "otherGamesRole":
                    addedOrRemoved = await DiscordHelper.AddOrRemoveRole("Other Games Pings", arg.User.Id);
                    roleName = "Other Games Pings";
                    break;
                case "foodEnjoyerRole":
                    addedOrRemoved = await DiscordHelper.AddOrRemoveRole("Food Enjoyer", arg.User.Id);
                    roleName = "Food Enjoyer";
                    break;

                default:
                    return;
            }

            if (addedOrRemoved == true)
            {
                await arg.FollowupAsync($"Your role **{roleName}** has been added! pooooooooo", ephemeral: true);
            }
            else
            {
                await arg.FollowupAsync($"Your role **{roleName}** has been remove!", ephemeral: true);
            }
        }

        public static async Task HandleGiveawayButtons(SocketMessageComponent arg)
        {
            if (arg.Channel.Id == AppConfig.DiscordGiveawayChannelID)
            {
                var response = await GiveawayData.HandleGiveawayButtonPressed(arg.User.Id, arg.Data.CustomId);

                await arg.FollowupAsync(response.Response, ephemeral: response.Ephemeral);
            }
        }
    }
}
