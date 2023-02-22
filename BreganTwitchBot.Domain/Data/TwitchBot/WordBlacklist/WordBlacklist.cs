using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.SuperMods;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot_Domain;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Domain.Data.TwitchBot.WordBlacklist
{
    public class WordBlacklist
    {
        private static List<string> _strikeWords = new();
        private static List<string> _warningWords = new();
        private static List<string> _tempBanWords = new();
        private static List<string> _permBanWords = new();
        private static List<string> _warnedUsers = new();
        private static List<string> _strikedUsers = new();
        private static int _messagesInARow;
        private static string _lastMessage = "";
        private static string _lastUser = "";

        public static void LoadBlacklistedWords()
        {
            using (var context = new DatabaseContext())
            {
                _warningWords = context.Blacklist.Where(x => x.WordType == "10sword").Select(x => x.Word).ToList();
                _tempBanWords = context.Blacklist.Where(x => x.WordType == "tempword").Select(x => x.Word).ToList();
                _permBanWords = context.Blacklist.Where(x => x.WordType == "word").Select(x => x.Word).ToList();
                _strikeWords = context.Blacklist.Where(x => x.WordType == "strikeword").Select(x => x.Word).ToList();
            }

            _warnedUsers = new List<string>();
            _strikedUsers = new List<string>();
        }

        public static async Task HandleMessageChecks(OnMessageReceivedArgs e)
        {
            //for the /me melvins
            if (e.ChatMessage.IsMe)
            {
                if (e.ChatMessage.Message.ToLower().StartsWith("has donated") || e.ChatMessage.Message.ToLower().StartsWith("donated"))
                {
                    await TwitchHelper.TimeoutUser(e.ChatMessage.UserId, 10, "blocksWOT automated rule - monopoly money melvin");
                    TwitchHelper.SendMessage("Thank you for the monopoly money blocksBANNED (Warning)");
                    Log.Information($"[Word Blacklist] Timed out {e.ChatMessage.Username} for melvin monopoly money");
                }
            }

            var message = Regex.Replace(e.ChatMessage.Message, @"[^0-9a-zA-Z⠁⠂⠃⠄⠅⠆⠇⠈⠉⠊⠋⠌⠍⠎⠏⠐⠑⠒⠓⠔⠕⠖⠗⠘⠙⠚⠛⠜⠝⠞⠟⠠⠡⠢⠣⠤⠥⠦⠧⠨⠩⠪⠫⠬⠭⠮⠯⠰⠱⠲⠳⠴⠵⠶⠷⠸⠹⠺⠻⠼⠽⠾⠿⡀⡁⡂⡃⡄⡅⡆⡇⡈⡉⡊⡋⡌⡍⡎⡏⡐⡑⡒⡓⡔⡕⡖⡗⡘⡙⡚⡛⡜⡝⡞⡟⡠⡡⡢⡣⡤⡥⡦⡧⡨⡩⡪⡫⡬⡭⡮⡯⡰⡱⡲⡳⡴⡵⡶⡷⡸⡹⡺⡻⡼⡽⡾⡿⢀⢁⢂⢃⢄⢅⢆⢇⢈⢉⢊⢋⢌⢍⢎⢏⢐⢑⢒⢓⢔⢕⢖⢗⢘⢙⢚⢛⢜⢝⢞⢟⢠⢡⢢⢣⢤⢥⢦⢧⢨⢩⢪⢫⢬⢭⢮⢯⢰⢱⢲⢳⢴⢵⢶⢷⢸⢹⢺⢻⢼⢽⢾⢿⣀⣁⣂⣃⣄⣅⣆⣇⣈⣉⣊⣋⣌⣍⣎⣏⣐⣑⣒⣓⣔⣕⣖⣗⣘⣙⣚⣛⣜⣝⣞⣟⣠⣡⣢⣣⣤⣥⣦⣧⣨⣩⣪⣫⣬⣭⣮⣯⣰⣱⣲⣳⣴⣵⣶⣷⣸⣹⣺⣻⣼⣽⣾⣿░█▄▌▀─\p{L}]+", "").ToLower();
            var rankBeggarRegex = Regex.Replace(e.ChatMessage.Message, @"[^0-9a-zA-Z+\p{L}]+", "").ToLower();

            //we trust these geezers
            if (e.ChatMessage.IsModerator || e.ChatMessage.IsBroadcaster || Supermods.IsUserSupermod(e.ChatMessage.UserId))
            {
                return;
            }

            if (e.ChatMessage.Username.ToLower() == _lastUser && e.ChatMessage.Message.ToLower() == _lastMessage)
            {
                _messagesInARow++;
                Log.Information($"[Anti Spam] Counter set to {_messagesInARow} for {_lastUser}");

                if (_messagesInARow == 3)
                {
                    await TwitchHelper.TimeoutUser(e.ChatMessage.UserId, 60, "vip spam");
                    TwitchHelper.SendMessage($"@{e.ChatMessage.Username} => Stop using your VIP to spam please :)");
                    _messagesInARow = 0;
                    _lastUser = "";
                    _lastMessage = "";
                    return;
                }
            }
            else
            {
                _lastUser = e.ChatMessage.Username.ToLower();
                _lastMessage = e.ChatMessage.Message.ToLower();
                _messagesInARow = 1;
            }

            if (AppConfig.AiEnabled)
            {
                RankBeggar.ModelInput sampleData = new RankBeggar.ModelInput()
                {
                    Message = e.ChatMessage.Message.Replace("'", "").Replace("blocksRank", "").Replace("blocksMe", "").Replace("blocksPls", "").Replace("blocksGiveaway", ""),
                };

                // Make a single prediction on the sample data and print results
                var predictionResult = RankBeggar.Predict(sampleData);

                if (predictionResult.AiResult == 1 || _strikeWords.Any(rankBeggarRegex.Contains))
                {
                    using (var context = new DatabaseContext())
                    {
                        context.RankBeggar.Add(new Infrastructure.Database.Models.RankBeggar
                        {
                            Message = e.ChatMessage.Message,
                            AiResult = 1
                        });

                        await context.SaveChangesAsync();
                    }

                    if (predictionResult.Score[1] < 0.70)
                    {
                        Log.Information($"[Word Blacklist] Did not time out {e.ChatMessage.Username} for begging, bot is unsure. Message - {e.ChatMessage.Message}");
                        return;
                    }

                    var totalMessages = GetTotalMessages(e.ChatMessage.UserId);

                    if (totalMessages < 10)
                    {
                        TwitchHelper.SendMessage("It's quite rude to join and beg within your first few messages blocksBANNED");
                        await TwitchHelper.BanUser(e.ChatMessage.UserId, "Joining and begging");
                        return;
                    }

                    if (totalMessages < 20)
                    {
                        if (_strikedUsers.Contains(e.ChatMessage.Username))
                        {
                            TwitchHelper.SendMessage("Warned once and continued to beg, very naughty blocksBANNED");
                            await TwitchHelper.BanUser(e.ChatMessage.UserId, "continued to beg after being warned for begging at low message count");
                            return;
                        }

                        TwitchHelper.SendMessage("How about you don't join the stream and start begging for ranks :) blocksBANNED");
                        await TwitchHelper.TimeoutUser(e.ChatMessage.UserId, 600, "blocksWOT automated AI rule - joining and begging");
                        await AddTimeoutStrike(e.ChatMessage.UserId);

                        _strikedUsers.Add(e.ChatMessage.Username);
                        Log.Information($"[Word Blacklist] Timed out {e.ChatMessage.Username} for begging with low message count");
                    }
                    else if (_warnedUsers.Contains(e.ChatMessage.Username.ToLower()))
                    {
                        TwitchHelper.SendMessage("I told you once not to beg and you do it again blocksRage blocksBANNED");
                        await TwitchHelper.TimeoutUser(e.ChatMessage.UserId, 600, "blocksWOT automated AI rule - multiple warnings");

                        Log.Information($"[Word Blacklist] Timed out {e.ChatMessage.Username} for begging");
                        await AddTimeoutStrike(e.ChatMessage.UserId);

                        var totalStrikes = GetTotalTimeoutStrikes(e.ChatMessage.Username);
                        var totalWarns = GetTotalWarns(e.ChatMessage.Username);

                        //todo: when discord added, send the above values
                        //await DiscordConnection.SendMessage(928407056086605845, $"{e.ChatMessage.Username} timed out \n Total Strikes So Far: {totalStrikes:N0} \n Total Warns: {totalWarns} \n Total Messages: {totalMessages}");
                    }
                    else
                    {
                        await TwitchHelper.DeleteMessage(e.ChatMessage.Id);
                        TwitchHelper.SendMessage("Please don't beg for ranks/ask for an f add! :) blocksBANNED");
                        _warnedUsers.Add(e.ChatMessage.Username.ToLower());
                        Log.Information($"[Word Blacklist] Warned {e.ChatMessage.Username} for begging/f add");
                        await AddWarn(e.ChatMessage.UserId);

                        var totalStrikes = GetTotalTimeoutStrikes(e.ChatMessage.UserId);
                        var totalWarns = GetTotalWarns(e.ChatMessage.UserId);

                        //todo: when discord added
                        //await DiscordConnection.SendMessage(928407056086605845, $"{e.ChatMessage.Username} warned \n Total Strikes So Far: {totalStrikes:N0} \n Total Warns: {totalWarns} \n Total Messages: {totalMessages}");
                    }
                }
                else
                {
                    using (var context = new DatabaseContext())
                    {
                        context.RankBeggar.Add(new Infrastructure.Database.Models.RankBeggar
                        {
                            Message = e.ChatMessage.Message,
                            AiResult = 0
                        });

                        context.SaveChanges();
                    }
                }
            }

            var messageToCheckForLinks = e.ChatMessage.Message.ToLower().Replace(" ", "");

            //for links/annoying stuff
            //clips are allowed
            if (messageToCheckForLinks.Contains("https://clips.twitch.tv"))
            {
                messageToCheckForLinks = messageToCheckForLinks.Replace("https://clips.twitch.tv", "");
            }

            if (_warningWords.Any(messageToCheckForLinks.Contains))
            {
                await TwitchHelper.TimeoutUser(e.ChatMessage.UserId, 30, "blocksWOT automated rule - link or promotion");
                TwitchHelper.SendMessage("Please don't post links or say that over streamers are live :) blocksBANNED");
                Log.Information($"[Word Blacklist] BAD WORD DETECTED {_warningWords.FirstOrDefault(e.ChatMessage.Message.ToLower().Replace(" ", "").Contains)} sent by {e.ChatMessage.Username} ");
            }

            //for 600s words
            if (_tempBanWords.Any(message.Contains))
            {
                await TwitchHelper.TimeoutUser(e.ChatMessage.UserId, 600, "blocksWOT automated rule - potentialy offensive word");
                TwitchHelper.SendMessage("Yeetus temp ban deletus blocksBANNED");
                Log.Information($"[Word Blacklist] BAD WORD DETECTED {_warningWords.FirstOrDefault(e.ChatMessage.Message.ToLower().Replace(" ", "").Contains)} sent by {e.ChatMessage.Username} ");
            }

            //for the edgy 12 year olds who think saying the n word is funny
            if (_permBanWords.Any(message.Contains))
            {
                await TwitchHelper.BanUser(e.ChatMessage.UserId, "blocksWOT automated rule - very offensive word");
                TwitchHelper.SendMessage("Yeetus perm ban deletus blocksBANNED");
                Log.Information($"[Word Blacklist] BAD WORD DETECTED {_warningWords.FirstOrDefault(e.ChatMessage.Message.ToLower().Replace(" ", "").Contains)} sent by {e.ChatMessage.Username} ");
            }
        }

        private static int GetTotalMessages(string userId)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.TwitchUserId == userId).FirstOrDefault();

                if (user == null)
                {
                    return 0;
                }

                return user.TotalMessages;
            }
        }

        private static async Task AddTimeoutStrike(string userId)
        {
            using (var context = new DatabaseContext())
            {
                context.Users.Where(x => x.TwitchUserId == userId).First().TimeoutStrikes++;
                await context.SaveChangesAsync();
            }
        }

        private static int GetTotalTimeoutStrikes(string userId)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.TwitchUserId == userId).FirstOrDefault();

                if (user == null)
                {
                    return 0;
                }

                return user.TimeoutStrikes;
            }
        }

        private static int GetTotalWarns(string userId)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.TwitchUserId == userId).FirstOrDefault();

                if (user == null)
                {
                    return 0;
                }

                return user.WarnStrikes;
            }
        }

        private static async Task AddWarn(string userId)
        {
            using (var context = new DatabaseContext())
            {
                context.Users.Where(x => x.TwitchUserId == userId).First().WarnStrikes++;
                await context.SaveChangesAsync();
            }
        }
    }
}
