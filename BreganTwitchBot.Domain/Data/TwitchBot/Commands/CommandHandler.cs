using BreganTwitchBot.Domain.Data.TwitchBot.Commands.DadJoke;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.StreamInfo;
using BreganTwitchBot.Domain.Data.TwitchBot.Commands.WordBlacklist;
using BreganTwitchBot.Domain.Data.TwitchBot.Enums;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Domain.Data.TwitchBot.WordBlacklist;
using BreganTwitchBot.Infrastructure.Database.Context;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands
{
    public class CommandHandler
    {
        public static async Task HandleCommand(OnChatCommandReceivedArgs command)
        {
            await StreamStatsService.UpdateStreamStat(1, StatTypes.CommandsSent);
            Log.Information($"[Twitch Commands] !{command.Command.CommandText.ToLower()} receieved from {command.Command.ChatMessage.Username}");

            //todo: do custom commands
            switch (command.Command.CommandText.ToLower())
            {
                case "dadjoke":
                    await DadJoke.DadJoke.HandleDadJokeCommand(command.Command.ChatMessage.Username);
                    break;
                case "8ball":
                    EightBall.EightBall.Handle8BallCommand(command.Command.ChatMessage.Username);
                    break;
                case "commands":
                    TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => Bot commands + much more can be found at https://bot.bregan.me/ ! :)");
                    break;
                case "uptime":
                    await Uptime.Uptime.HandleUptimeCommand(command.Command.ChatMessage.Username, command.Command.ChatMessage.UserId);
                    break;
                case "botuptime":
                    Uptime.Uptime.HandleBotUptimeCommand(command.Command.ChatMessage.Username);
                    break;
                case "shoutout" when command.Command.ChatMessage.IsModerator:
                case "shoutout" when command.Command.ChatMessage.IsBroadcaster:
                case "so" when command.Command.ChatMessage.IsModerator:
                case "so" when command.Command.ChatMessage.IsBroadcaster:
                    if (command.Command.ArgumentsAsList.Count == 0)
                    {
                        return;
                    }

                    TwitchHelper.SendMessage($"Hey go check out {command.Command.ArgumentsAsList[0].Replace("@", "")} at twitch.tv/{command.Command.ArgumentsAsList[0].Replace("@", "").Trim()} for some great content!");
                    break;
                case "addcmd" when command.Command.ChatMessage.IsModerator:
                case "addcmd" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "cmdadd" when command.Command.ChatMessage.IsModerator:
                case "cmdadd" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                    await CustomCommands.CustomCommands.HandleAddCommand(command.Command.ChatMessage.Username, command.Command.ArgumentsAsList, command.Command.ArgumentsAsString);
                    break;
                case "delcmd" when command.Command.ChatMessage.IsModerator:
                case "delcmd" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "cmddel" when command.Command.ChatMessage.IsModerator:
                case "cmddel" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                    await CustomCommands.CustomCommands.HandleRemoveCommand(command.Command.ChatMessage.Username, command.Command.CommandText.ToLower());
                    break;
                case "editcmd" when command.Command.ChatMessage.IsModerator:
                case "editcmd" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "cmdedit" when command.Command.ChatMessage.IsModerator:
                case "cmdedit" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                    await CustomCommands.CustomCommands.HandleEditCommandCommand(command.Command.ChatMessage.Username, command.Command.ArgumentsAsList, command.Command.ArgumentsAsString);
                    break;
                case "title":
                    await Title.HandleTitleCommand(command.Command.ChatMessage.Username, command.Command.ChatMessage.Message, command.Command.ArgumentsAsString, command.Command.ChatMessage.IsModerator, command.Command.ChatMessage.IsBroadcaster);
                    break;
                case "game":
                    await Game.HandleGameCommand(command.Command.ChatMessage.Username, command.Command.ChatMessage.Message, command.Command.ArgumentsAsString, command.Command.ChatMessage.IsModerator, command.Command.ChatMessage.IsBroadcaster);
                    break;
                case "subs":
                case "subcount":
                    await Subs.HandleSubsCommand(command.Command.ChatMessage.Username);
                    break;
                default:
                    break;
                case "followage":
                case "howlong":
                    await FollowAge.FollowAge.HandleFollowageCommand(command.Command.ChatMessage.Username, command.Command.ChatMessage.UserId, command.Command.ChatMessage.Message, command.Command.ArgumentsAsList);
                    break;
                case "followsince":
                    await FollowAge.FollowAge.HandleFollowSinceCommand(command.Command.ChatMessage.Username, command.Command.ChatMessage.UserId, command.Command.ChatMessage.Message, command.Command.ArgumentsAsList);
                    break;
                case "points":
                case "pooants":
                case "coins":
                    Points.Points.HandlePointsCommand(command.Command.ChatMessage.Username, command.Command.ChatMessage.UserId, command.Command.ChatMessage.Message, command.Command.ArgumentsAsList);
                    break;
                case "hours":
                case "houres":
                case "hrs":
                case "watchtime":
                    Hours.Hours.HandleHoursCommand(command.Command.ChatMessage.Username, command.Command.ChatMessage.UserId, command.Command.ChatMessage.Message, command.Command.ArgumentsAsList);
                    break;
                case "streamhours":
                case "streamhrs":
                    Hours.Hours.HandleStreamHoursCommand(command.Command.ChatMessage.Username, command.Command.ChatMessage.UserId, command.Command.ChatMessage.Message, command.Command.ArgumentsAsList);
                    break;
                case "weeklyhours":
                case "weeklyhrs":
                    Hours.Hours.HandleWeeklyHoursCommand(command.Command.ChatMessage.Username, command.Command.ChatMessage.UserId, command.Command.ChatMessage.Message, command.Command.ArgumentsAsList);
                    break;
                case "monthlyhours":
                case "monthlyhrs":
                    Hours.Hours.HandleMonthlyHoursCommand(command.Command.ChatMessage.Username, command.Command.ChatMessage.UserId, command.Command.ChatMessage.Message, command.Command.ArgumentsAsList);
                    break;
                case "addbadword" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "addtempword" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "addwarningword" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "addstrikeword" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                    await WordBlacklistCommand.HandleAddWordCommand(command.Command.ChatMessage.Username, command.Command.CommandText, command.Command.ArgumentsAsString);
                    break;
                case "removebadword" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "removetempword" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "removewarningword" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "removestrikeword" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                    await WordBlacklistCommand.HandleRemoveWordCommand(command.Command.ChatMessage.Username, command.Command.CommandText, command.Command.ArgumentsAsString);
                    break;
                case "aitoggle" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                case "toggleai" when TwitchHelper.IsUserSupermod(command.Command.ChatMessage.UserId):
                    WordBlacklistCommand.HandleToggleAiCommand(command.Command.ChatMessage.Username);
                    break;
                case "spin":
                case "slots":
                case "gamble":
                    await Gambling.Gambling.HandleGamblingCommand(command.Command.ChatMessage.Username, command.Command.ChatMessage.UserId, command.Command.ChatMessage.Message, command.Command.ArgumentsAsList);
                    break;
                case "spinstats":
                    Gambling.Gambling.HandleSpinStatsCommand(command.Command.ChatMessage.Username);
                    break;
            }
        }

        public static async Task HandleCustomCommand(string? commandName, string username, string userId)
        {
            //Commands beginning with ! are already handled below
            if (commandName == null)
            {
                return;
            }

            if (commandName.StartsWith("!"))
            {
                return;
            }

            using (var context = new DatabaseContext())
            {
                //get the command and check if it exists
                var command = context.Commands.Where(x => x.CommandName == commandName).FirstOrDefault();

                if (command == null)
                {
                    return;
                }

                //Make sure it's not on a cooldown
                if (DateTime.UtcNow - TimeSpan.FromSeconds(5) < command.LastUsed && !Supermods.IsUserSupermod(userId))
                {
                    Log.Information("[Twitch Commands] Custom command handled successfully (cooldown)");
                    return;
                }

                await StreamStatsService.UpdateStreamStat(1, StatTypes.CommandsSent);

                //Send the message
                TwitchHelper.SendMessage(command.CommandText.Replace("[count]", command.TimesUsed.ToString("N0")).Replace("[user]", username));

                //Update the command usage and time
                command.LastUsed = DateTime.UtcNow;
                command.TimesUsed++;
                await context.SaveChangesAsync();

                Log.Information($"[Twitch Commands] Custom command {commandName} handled successfully");
            }
        }
    }
}
