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