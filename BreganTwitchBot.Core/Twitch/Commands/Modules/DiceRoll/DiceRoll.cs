using BreganTwitchBot.Data;
using BreganTwitchBot.Twitch.Helpers;
using Serilog;
using TwitchLib.Client.Events;

namespace BreganTwitchBot.Core.Twitch.Commands.Modules.DiceRoll
{
    public class DiceRoll
    {
        public static void HandleDiceCommand(string username)
        {
            var normalRollsLeft = 0;
            var bonusRollsLeft = 0;

            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.Username == username).FirstOrDefault();

                if (user != null)
                {
                    normalRollsLeft = user.DiceRolls;
                    bonusRollsLeft = user.BonusDiceRolls;
                }
            }

            if (normalRollsLeft > 0)
            {
                CompleteNormalRoll(username);
                using (var context = new DatabaseContext())
                {
                    context.Users.Where(x => x.Username == username).First().DiceRolls -= 1;
                    context.SaveChanges();
                }
                return;
            }

            if (bonusRollsLeft > 0)
            {
                CompleteBonusRoll(username);
                using (var context = new DatabaseContext())
                {
                    context.Users.Where(x => x.Username == username).First().BonusDiceRolls -= 1;
                    context.SaveChanges();
                }
                return;
            }
        }

        public static void HandleAddRollCommand(OnChatCommandReceivedArgs command)
        {
            if (command.Command.ChatMessage.Message == "!addroll")
            {
                TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => The usage for this command is !addroll <user name>");
                Log.Information("[Twitch Commands] !addroll command handled successfully (advice)");
                return;
            }

            AddNormalDiceRoll(command.Command.ArgumentsAsList[0].ToLower());
            TwitchHelper.SendMessage($"@{command.Command.ChatMessage.Username} => The roll should be added");
        }

        public static void AddNormalDiceRoll(string username)
        {
            using (var context = new DatabaseContext())
            {
                context.Users.Where(x => x.Username == username).First().DiceRolls++;
                context.SaveChanges();
            }
            Log.Information($"[Dice Roll] Dice roll added to {username}");
        }

        private static void CompleteNormalRoll(string username)
        {
            var random = new Random();

            switch (random.Next(1, 7))
            {
                case 1:
                    TwitchHelper.SendMessage($"@{username} => You have rolled a 1. yerrrrrrrrrrrrrrrrrrrrrrrrrrrr banned 600 seconds in the cage 4Head");
                    TwitchHelper.TimeoutUser(username, TimeSpan.FromSeconds(600), "geezer rolled a 1 KEKW");
                    break;
                case 2:
                    TwitchHelper.SendMessage($"@{username} => You have rolled a 2. PogChamp PogChamp PogChamp PogChamp PogChamp PogChamp PogChamp PogChamp you won nothing!");
                    break;
                case 3:
                    switch (random.Next(1, 4))
                    {
                        case 1:
                            TwitchHelper.SendMessage($"@{username} => You have rolled a 3. You have won some sunglasses! @blocksssssss get one of your fancy pairs on 4Head");
                            break;
                        case 2:
                            TwitchHelper.SendMessage($"@{username} => You have rolled a 3. Now this is content! @blocksssssss better get that camera massive as {username} has won FeelsBadMan LUL");
                            break;
                        case 3:
                            //todo: this
                            break;
                    }
                    break;
                case 4:
                    TwitchHelper.SendMessage($"@{username} => You have rolled a 4. Enjoy VIP for the rest of the stream PogChamp @blocksssssss hook this geezer up with some VIP action");
                    break;
                case 5:
                    switch (random.Next(1, 4))
                    {
                        case 1:
                            TwitchHelper.SendMessage($"@{username} => You have rolled a 5. not sure what it even means but now @blocksssssss has to polygot ??? I am confused lol");
                            break;
                        case 2:
                            TwitchHelper.SendMessage($"@{username} => You have rolled a 5. You get your roll back");
                            AddNormalDiceRoll(username);
                            break;
                        case 3:
                            TwitchHelper.SendMessage($"@{username} => You have rolled a 5. By far the best prize out there! Enjoy your game of Farm Hunt @blocksssssss KEKW");
                            break;
                    }
                    break;
                case 6:
                    TwitchHelper.SendMessage($"@{username} => You have rolled a 6. 🚨 DING DING DING 🚨 You have won a bonus roll PogChamp do !roll again to test your luck");
                    using (var context = new DatabaseContext())
                    {
                        context.Users.Where(x => x.Username == username).First().BonusDiceRolls++;
                        context.SaveChanges();
                    }
                    break;
            }
        }

        private static void CompleteBonusRoll(string username)
        {
            var random = new Random();

            switch (random.Next(1, 25))
            {
                case 1:
                    TwitchHelper.SendMessage($"@{username} => You got this far all to get banned for 600 seconds OMEGALUL enjoy the time in the cage bruv");
                    TwitchHelper.TimeoutUser(username, TimeSpan.FromSeconds(600), "geezer rolled a 1 on a bonus roll KEKW creasing");
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                //maybe something here
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                    TwitchHelper.SendMessage($"@{username} => You get to decide who to raid PogChamp Which streamer with the banging content will be raiding at the end of the stream? Better not be fotelive KEKW");
                    break;
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                //maybe something here
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                    TwitchHelper.SendMessage($"@{username} => You have won 2 things PogChamp You a custom bot command AND you get to pick the next game to play (Farm Hunt please :) or I will be a very sad bot :( You don't want to make me sad do you? :( :( )");
                    break;
                case 22:
                    TwitchHelper.SendMessage($"@{username} => Now we're talking! You have just won a proper banging VIP for a whooooooooooooooooooooooooooooooooooole week. Proper fancy aint it - you get a Discord rank and everything");
                    break;
                case 23:
                case 24:
                    TwitchHelper.SendMessage($"@{username} => You have basicially hit the yackpot here! You have won a gifted sub blocksGuinea If you are silly enough to be subbed you can choose to gift it to another person or redeem it once your sub expires PogChamp On top of that you also win a dad yoke: no you dont as this code doesnt work :)");
                    break;
            }
        }
    }
}
