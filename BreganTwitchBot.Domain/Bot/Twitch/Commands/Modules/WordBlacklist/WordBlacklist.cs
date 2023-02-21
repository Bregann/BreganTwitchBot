using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.SuperMods;
using Serilog;
using System.Text.RegularExpressions;
using TwitchLib.Client.Events;
using BreganTwitchBot_Domain;
using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;

namespace BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.WordBlacklist
{
    public class WordBlacklist
    {
        private static List<string> _strikeWords;
        private static List<string> _warningWords;
        private static List<string> _tempBanWords;
        private static List<string> _permBanWords;
        private static List<string> _warnedUsers;
        private static List<string> _strikedUsers;
        private static int _messagesInARow;
        private static string _lastMessage = "";
        private static string _lastUser = "";
        private static bool aiEnabled = true;

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

        public static void HandleToggleAiCommand(OnChatCommandReceivedArgs command)
        {
            if (aiEnabled == true)
            {
                aiEnabled = false;
            }
            else
            {
                aiEnabled = true;
            }

            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => The AI rank begging has been set to {aiEnabled}!");
        }

        public static void HandleAddWordCommand(OnChatCommandReceivedArgs command)
        {
            if (command.Command.ArgumentsAsString == "")
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => The usage for this command is !{command.Command.CommandText} <word>");
                Log.Information("[Twitch Commands] !addbadword command handled successfully");
                return;
            }

            var words = Regex.Replace(command.Command.ArgumentsAsString, @"[^0-9a-zA-Z⠁⠂⠃⠄⠅⠆⠇⠈⠉⠊⠋⠌⠍⠎⠏⠐⠑⠒⠓⠔⠕⠖⠗⠘⠙⠚⠛⠜⠝⠞⠟⠠⠡⠢⠣⠤⠥⠦⠧⠨⠩⠪⠫⠬⠭⠮⠯⠰⠱⠲⠳⠴⠵⠶⠷⠸⠹⠺⠻⠼⠽⠾⠿⡀⡁⡂⡃⡄⡅⡆⡇⡈⡉⡊⡋⡌⡍⡎⡏⡐⡑⡒⡓⡔⡕⡖⡗⡘⡙⡚⡛⡜⡝⡞⡟⡠⡡⡢⡣⡤⡥⡦⡧⡨⡩⡪⡫⡬⡭⡮⡯⡰⡱⡲⡳⡴⡵⡶⡷⡸⡹⡺⡻⡼⡽⡾⡿⢀⢁⢂⢃⢄⢅⢆⢇⢈⢉⢊⢋⢌⢍⢎⢏⢐⢑⢒⢓⢔⢕⢖⢗⢘⢙⢚⢛⢜⢝⢞⢟⢠⢡⢢⢣⢤⢥⢦⢧⢨⢩⢪⢫⢬⢭⢮⢯⢰⢱⢲⢳⢴⢵⢶⢷⢸⢹⢺⢻⢼⢽⢾⢿⣀⣁⣂⣃⣄⣅⣆⣇⣈⣉⣊⣋⣌⣍⣎⣏⣐⣑⣒⣓⣔⣕⣖⣗⣘⣙⣚⣛⣜⣝⣞⣟⣠⣡⣢⣣⣤⣥⣦⣧⣨⣩⣪⣫⣬⣭⣮⣯⣰⣱⣲⣳⣴⣵⣶⣷⣸⣹⣺⣻⣼⣽⣾⣿░█▄▌▀─\p{L}]+", "").ToLower();

            switch (command.Command.CommandText.ToLower())
            {
                case "addbadword":
                    if (_permBanWords.Contains(words))
                    {
                        TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => You silly ole bean that word is already blacklisted!");
                        return;
                    }
                    AddBlacklistedItem(words, "word");
                    _permBanWords.Add(words);

                    TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => That word has been blacklisted :)");
                    break;

                case "addtempword":
                    if (_tempBanWords.Contains(words))
                    {
                        TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => You silly ole bean that word is already blacklisted!");
                        return;
                    }
                    AddBlacklistedItem(words, "tempword");
                    _tempBanWords.Add(words);

                    TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => That word has been blacklisted :)");
                    break;

                case "addwarningword":
                    if (_warningWords.Contains(command.Command.ArgumentsAsString))
                    {
                        TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => You silly ole bean that word is already blacklisted!");
                        return;
                    }
                    AddBlacklistedItem(command.Command.ArgumentsAsString, "10sword");
                    _warningWords.Add(command.Command.ArgumentsAsString);

                    TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => That word has been blacklisted :)");
                    break;

                case "addstrikeword":
                    if (_strikeWords.Contains(command.Command.ArgumentsAsString))
                    {
                        TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => You silly ole bean that word is already blacklisted!");
                        return;
                    }
                    AddBlacklistedItem(words, "strikeword");
                    _strikeWords.Add(words);

                    TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => That word has been blacklisted :)");
                    break;
            }

            Log.Information($"[Word Blacklist] {command.Command.ArgumentsAsString} has been blacklisted");
        }

        public static void HandleRemoveWordCommand(OnChatCommandReceivedArgs command)
        {
            if (command.Command.ArgumentsAsString == "")
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => The usage for this command is !{command.Command.CommandText} <word>");
                Log.Information("[Twitch Commands] !addbadword command handled successfully");
                return;
            }

            var words = Regex.Replace(command.Command.ArgumentsAsString, @"[^0-9a-zA-Z⠁⠂⠃⠄⠅⠆⠇⠈⠉⠊⠋⠌⠍⠎⠏⠐⠑⠒⠓⠔⠕⠖⠗⠘⠙⠚⠛⠜⠝⠞⠟⠠⠡⠢⠣⠤⠥⠦⠧⠨⠩⠪⠫⠬⠭⠮⠯⠰⠱⠲⠳⠴⠵⠶⠷⠸⠹⠺⠻⠼⠽⠾⠿⡀⡁⡂⡃⡄⡅⡆⡇⡈⡉⡊⡋⡌⡍⡎⡏⡐⡑⡒⡓⡔⡕⡖⡗⡘⡙⡚⡛⡜⡝⡞⡟⡠⡡⡢⡣⡤⡥⡦⡧⡨⡩⡪⡫⡬⡭⡮⡯⡰⡱⡲⡳⡴⡵⡶⡷⡸⡹⡺⡻⡼⡽⡾⡿⢀⢁⢂⢃⢄⢅⢆⢇⢈⢉⢊⢋⢌⢍⢎⢏⢐⢑⢒⢓⢔⢕⢖⢗⢘⢙⢚⢛⢜⢝⢞⢟⢠⢡⢢⢣⢤⢥⢦⢧⢨⢩⢪⢫⢬⢭⢮⢯⢰⢱⢲⢳⢴⢵⢶⢷⢸⢹⢺⢻⢼⢽⢾⢿⣀⣁⣂⣃⣄⣅⣆⣇⣈⣉⣊⣋⣌⣍⣎⣏⣐⣑⣒⣓⣔⣕⣖⣗⣘⣙⣚⣛⣜⣝⣞⣟⣠⣡⣢⣣⣤⣥⣦⣧⣨⣩⣪⣫⣬⣭⣮⣯⣰⣱⣲⣳⣴⣵⣶⣷⣸⣹⣺⣻⣼⣽⣾⣿░█▄▌▀─\p{L}]+", "").ToLower();

            switch (command.Command.CommandText.ToLower())
            {
                case "removebadword":
                    if (!_permBanWords.Contains(words))
                    {
                        TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => Oh dear that word isn't blacklisted you silly sausage!");
                        Log.Information("[Twitch Commands] !removebadword command handled successfully");
                        return;
                    }

                    RemoveBlacklistedItem(words, "word");
                    _permBanWords.Remove(words);

                    TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => That word has been unblacklisted :)");
                    Log.Information("[Twitch Commands] !removebadword command handled successfully");
                    break;

                case "removetempword":
                    if (!_tempBanWords.Contains(words))
                    {
                        TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => Oh dear that word isn't blacklisted you silly sausage!");
                        Log.Information("[Twitch Commands] !removetempword command handled successfully");
                        return;
                    }

                    RemoveBlacklistedItem(words, "tempword");
                    _tempBanWords.Remove(words);

                    TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => That word has been unblacklisted :)");
                    Log.Information("[Twitch Commands] !removetempword command handled successfully");
                    break;

                case "removewarningword":
                    if (!_warningWords.Contains(command.Command.ArgumentsAsString))
                    {
                        TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => Oh dear that word isn't blacklisted you silly sausage!");
                        Log.Information("[Twitch Commands] !removewarningword command handled successfully");
                        return;
                    }

                    RemoveBlacklistedItem(command.Command.ArgumentsAsString, "10sword");
                    _warningWords.Remove(command.Command.ArgumentsAsString);

                    TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => That word has been unblacklisted :)");
                    Log.Information("[Twitch Commands] !removewarningword command handled successfully");
                    break;

                case "removestrikeword":
                    if (!_strikeWords.Contains(words))
                    {
                        TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => Oh dear that word isn't blacklisted you silly sausage!");
                        Log.Information("[Twitch Commands] !removestrikeword command handled successfully");
                        return;
                    }
                    RemoveBlacklistedItem(words, "strikeword");
                    _strikeWords.Remove(words);

                    TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => That word has been unblacklisted :)");
                    Log.Information("[Twitch Commands] !removestrikeword command handled successfully");
                    break;
            }

            Log.Information($"[Word Blacklist] {command.Command.ArgumentsAsString} has been blacklisted");
        }

        public static async void OnMessageReceived(OnMessageReceivedArgs e)
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

            if (aiEnabled)
            {
                RankBeggar.ModelInput sampleData = new RankBeggar.ModelInput()
                {
                    Message = e.ChatMessage.Message.Replace("'", "").Replace("blocksRank", "").Replace("blocksMe", "").Replace("blocksPls", "").Replace("blocksGiveaway", ""),
                };

                // Make a single prediction on the sample data and print results
                var predictionResult = RankBeggar.Predict(sampleData);

                if (predictionResult.AiResult == 1 || _strikeWords.Any(rankBeggarRegex.Contains))
                {
                    Log.Information($"debug - {predictionResult.Score[0]} - {predictionResult.Score[1]} - {predictionResult.Score.Length}");

                    using (var context = new DatabaseContext())
                    {
                        context.RankBeggar.Add(new Infrastructure.Database.Models.RankBeggar
                        {
                            Message = e.ChatMessage.Message,
                            AiResult = 1
                        });

                        context.SaveChanges();
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
                        AddTimeoutStrike(e.ChatMessage.UserId);

                        _strikedUsers.Add(e.ChatMessage.Username);
                        Log.Information($"[Word Blacklist] Timed out {e.ChatMessage.Username} for begging with low message count");
                    }
                    else if (_warnedUsers.Contains(e.ChatMessage.Username.ToLower()))
                    {
                        TwitchHelper.SendMessage("I told you once not to beg and you do it again blocksRage blocksBANNED");
                        await TwitchHelper.TimeoutUser(e.ChatMessage.UserId, 600, "blocksWOT automated AI rule - multiple warnings");
                        Log.Information($"[Word Blacklist] Timed out {e.ChatMessage.Username} for begging");
                        AddTimeoutStrike(e.ChatMessage.UserId);

                        var totalStrikes = GetTotalTimeoutStrikes(e.ChatMessage.Username);
                        var totalWarns = GetTotalWarns(e.ChatMessage.Username);

                        //todo: when discord added
                        //await DiscordConnection.SendMessage(928407056086605845, $"{e.ChatMessage.Username} timed out \n Total Strikes So Far: {totalStrikes:N0} \n Total Warns: {totalWarns} \n Total Messages: {totalMessages}");
                    }
                    else
                    {
                        await TwitchHelper.DeleteMessage(e.ChatMessage.Id);
                        TwitchHelper.SendMessage("Please don't beg for ranks/ask for an f add! :) blocksBANNED");
                        _warnedUsers.Add(e.ChatMessage.Username.ToLower());
                        Log.Information($"[Word Blacklist] Warned {e.ChatMessage.Username} for begging/f add");
                        AddWarn(e.ChatMessage.UserId);

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

        private static void AddTimeoutStrike(string userId)
        {
            using (var context = new DatabaseContext())
            {
                context.Users.Where(x => x.TwitchUserId == userId).First().TimeoutStrikes++;
                context.SaveChanges();
            }
        }

        private static void AddWarn(string userId)
        {
            using (var context = new DatabaseContext())
            {
                context.Users.Where(x => x.TwitchUserId == userId).First().WarnStrikes++;
                context.SaveChanges();
            }
        }

        private static void AddBlacklistedItem(string word, string type)
        {
            using (var context = new DatabaseContext())
            {
                context.Blacklist.Add(new Infrastructure.Database.Models.Blacklist
                {
                    WordType = type,
                    Word = word
                });

                context.SaveChanges();
            }
        }

        private static void RemoveBlacklistedItem(string word, string type)
        {
            using (var context = new DatabaseContext())
            {
                var wordToRemove = new Infrastructure.Database.Models.Blacklist
                {
                    Word = word,
                    WordType = type
                };

                context.Blacklist.Remove(wordToRemove);
                context.SaveChanges();
            }
        }

        public static void ClearOutWarnedUsers()
        {
            _warnedUsers.Clear();
            Log.Information("[Word Blacklist] Cleared out warned users list");
        }

        public static void ClearOutStrikedUsers()
        {
            _strikedUsers.Clear();
            Log.Information("[Word Blacklist] Cleared out striked users list");
        }
    }
}