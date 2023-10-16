using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Domain.Data.TwitchBot.WordBlacklist;
using BreganTwitchBot.Infrastructure.Database.Enums;
using Serilog;
using System.Text.RegularExpressions;

namespace BreganTwitchBot.Domain.Data.TwitchBot.Commands.WordBlacklist
{
    public class WordBlacklistCommand
    {
        public static async Task HandleAddWordCommand(string username, string commandName, string commandArguments)
        {
            if (commandArguments == "")
            {
                TwitchHelper.SendMessage($"@{username} => The usage for this command is !{commandName} <word>");
                Log.Information("[Twitch Commands] !addbadword command handled successfully");
                return;
            }

            var words = Regex.Replace(commandArguments, @"[^0-9a-zA-Z⠁⠂⠃⠄⠅⠆⠇⠈⠉⠊⠋⠌⠍⠎⠏⠐⠑⠒⠓⠔⠕⠖⠗⠘⠙⠚⠛⠜⠝⠞⠟⠠⠡⠢⠣⠤⠥⠦⠧⠨⠩⠪⠫⠬⠭⠮⠯⠰⠱⠲⠳⠴⠵⠶⠷⠸⠹⠺⠻⠼⠽⠾⠿⡀⡁⡂⡃⡄⡅⡆⡇⡈⡉⡊⡋⡌⡍⡎⡏⡐⡑⡒⡓⡔⡕⡖⡗⡘⡙⡚⡛⡜⡝⡞⡟⡠⡡⡢⡣⡤⡥⡦⡧⡨⡩⡪⡫⡬⡭⡮⡯⡰⡱⡲⡳⡴⡵⡶⡷⡸⡹⡺⡻⡼⡽⡾⡿⢀⢁⢂⢃⢄⢅⢆⢇⢈⢉⢊⢋⢌⢍⢎⢏⢐⢑⢒⢓⢔⢕⢖⢗⢘⢙⢚⢛⢜⢝⢞⢟⢠⢡⢢⢣⢤⢥⢦⢧⢨⢩⢪⢫⢬⢭⢮⢯⢰⢱⢲⢳⢴⢵⢶⢷⢸⢹⢺⢻⢼⢽⢾⢿⣀⣁⣂⣃⣄⣅⣆⣇⣈⣉⣊⣋⣌⣍⣎⣏⣐⣑⣒⣓⣔⣕⣖⣗⣘⣙⣚⣛⣜⣝⣞⣟⣠⣡⣢⣣⣤⣥⣦⣧⣨⣩⣪⣫⣬⣭⣮⣯⣰⣱⣲⣳⣴⣵⣶⣷⣸⣹⣺⣻⣼⣽⣾⣿░█▄▌▀─\p{L}]+", "").ToLower();

            switch (commandName.ToLower())
            {
                case "addbadword":
                    if (WordBlacklistData.CheckIfWordIsBlacklisted(words, WordTypes.PermBanWord))
                    {
                        TwitchHelper.SendMessage($"@{username} => You silly ole bean that word is already blacklisted!");
                        return;
                    }
                    await WordBlacklistData.AddBlacklistedItem(words, WordTypes.PermBanWord);

                    TwitchHelper.SendMessage($"@{username} => That word has been blacklisted :)");
                    break;

                case "addtempword":
                    if (WordBlacklistData.CheckIfWordIsBlacklisted(words, WordTypes.TempBanWord))
                    {
                        TwitchHelper.SendMessage($"@{username} => You silly ole bean that word is already blacklisted!");
                        return;
                    }
                    await WordBlacklistData.AddBlacklistedItem(words, WordTypes.TempBanWord);

                    TwitchHelper.SendMessage($"@{username} => That word has been blacklisted :)");
                    break;

                case "addwarningword":
                    if (WordBlacklistData.CheckIfWordIsBlacklisted(words, WordTypes.WarningWord))
                    {
                        TwitchHelper.SendMessage($"@{username} => You silly ole bean that word is already blacklisted!");
                        return;
                    }
                    await WordBlacklistData.AddBlacklistedItem(commandArguments, WordTypes.WarningWord);

                    TwitchHelper.SendMessage($"@{username} => That word has been blacklisted :)");
                    break;

                case "addstrikeword":
                    if (WordBlacklistData.CheckIfWordIsBlacklisted(words, WordTypes.StrikeWord))
                    {
                        TwitchHelper.SendMessage($"@{username} => You silly ole bean that word is already blacklisted!");
                        return;
                    }
                    await WordBlacklistData.AddBlacklistedItem(words, WordTypes.StrikeWord);

                    TwitchHelper.SendMessage($"@{username} => That word has been blacklisted :)");
                    break;
            }

            Log.Information($"[Word Blacklist] {commandArguments} has been blacklisted");
        }

        public static async Task HandleRemoveWordCommand(string username, string commandName, string commandArguments)
        {
            if (commandArguments == "")
            {
                TwitchHelper.SendMessage($"@{username} => The usage for this command is !{commandName} <word>");
                Log.Information("[Twitch Commands] !addbadword command handled successfully");
                return;
            }

            var words = Regex.Replace(commandArguments, @"[^0-9a-zA-Z⠁⠂⠃⠄⠅⠆⠇⠈⠉⠊⠋⠌⠍⠎⠏⠐⠑⠒⠓⠔⠕⠖⠗⠘⠙⠚⠛⠜⠝⠞⠟⠠⠡⠢⠣⠤⠥⠦⠧⠨⠩⠪⠫⠬⠭⠮⠯⠰⠱⠲⠳⠴⠵⠶⠷⠸⠹⠺⠻⠼⠽⠾⠿⡀⡁⡂⡃⡄⡅⡆⡇⡈⡉⡊⡋⡌⡍⡎⡏⡐⡑⡒⡓⡔⡕⡖⡗⡘⡙⡚⡛⡜⡝⡞⡟⡠⡡⡢⡣⡤⡥⡦⡧⡨⡩⡪⡫⡬⡭⡮⡯⡰⡱⡲⡳⡴⡵⡶⡷⡸⡹⡺⡻⡼⡽⡾⡿⢀⢁⢂⢃⢄⢅⢆⢇⢈⢉⢊⢋⢌⢍⢎⢏⢐⢑⢒⢓⢔⢕⢖⢗⢘⢙⢚⢛⢜⢝⢞⢟⢠⢡⢢⢣⢤⢥⢦⢧⢨⢩⢪⢫⢬⢭⢮⢯⢰⢱⢲⢳⢴⢵⢶⢷⢸⢹⢺⢻⢼⢽⢾⢿⣀⣁⣂⣃⣄⣅⣆⣇⣈⣉⣊⣋⣌⣍⣎⣏⣐⣑⣒⣓⣔⣕⣖⣗⣘⣙⣚⣛⣜⣝⣞⣟⣠⣡⣢⣣⣤⣥⣦⣧⣨⣩⣪⣫⣬⣭⣮⣯⣰⣱⣲⣳⣴⣵⣶⣷⣸⣹⣺⣻⣼⣽⣾⣿░█▄▌▀─\p{L}]+", "").ToLower();

            switch (commandName.ToLower())
            {
                case "removebadword":
                    if (!WordBlacklistData.CheckIfWordIsBlacklisted(words, WordTypes.PermBanWord))
                    {
                        TwitchHelper.SendMessage($"@{username} => Oh dear that word isn't blacklisted you silly sausage!");
                        Log.Information("[Twitch Commands] !removebadword command handled successfully");
                        return;
                    }

                    await WordBlacklistData.RemoveBlacklistedItem(words, WordTypes.PermBanWord);

                    TwitchHelper.SendMessage($"@{username} => That word has been unblacklisted :)");
                    Log.Information("[Twitch Commands] !removebadword command handled successfully");
                    break;

                case "removetempword":
                    if (!WordBlacklistData.CheckIfWordIsBlacklisted(words, WordTypes.TempBanWord))
                    {
                        TwitchHelper.SendMessage($"@{username} => Oh dear that word isn't blacklisted you silly sausage!");
                        Log.Information("[Twitch Commands] !removetempword command handled successfully");
                        return;
                    }

                    await WordBlacklistData.RemoveBlacklistedItem(words, WordTypes.TempBanWord);

                    TwitchHelper.SendMessage($"@{username} => That word has been unblacklisted :)");
                    Log.Information("[Twitch Commands] !removetempword command handled successfully");
                    break;

                case "removewarningword":
                    if (!WordBlacklistData.CheckIfWordIsBlacklisted(commandArguments, WordTypes.WarningWord))
                    {
                        TwitchHelper.SendMessage($"@{username} => Oh dear that word isn't blacklisted you silly sausage!");
                        Log.Information("[Twitch Commands] !removewarningword command handled successfully");
                        return;
                    }

                    await WordBlacklistData.RemoveBlacklistedItem(commandArguments, WordTypes.WarningWord);

                    TwitchHelper.SendMessage($"@{username} => That word has been unblacklisted :)");
                    Log.Information("[Twitch Commands] !removewarningword command handled successfully");
                    break;

                case "removestrikeword":
                    if (!WordBlacklistData.CheckIfWordIsBlacklisted(words, WordTypes.StrikeWord))
                    {
                        TwitchHelper.SendMessage($"@{username} => Oh dear that word isn't blacklisted you silly sausage!");
                        Log.Information("[Twitch Commands] !removestrikeword command handled successfully");
                        return;
                    }

                    await WordBlacklistData.RemoveBlacklistedItem(words, WordTypes.StrikeWord);

                    TwitchHelper.SendMessage($"@{username} => That word has been unblacklisted :)");
                    Log.Information("[Twitch Commands] !removestrikeword command handled successfully");
                    break;
            }

            Log.Information($"[Word Blacklist] {commandArguments} has been blacklisted");
        }
    }
}