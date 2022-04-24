using BreganTwitchBot.Data;
using BreganTwitchBot.DiscordBot.Helpers;
using Discord;
using Discord.Interactions;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using Color = Discord.Color;
using Image = SixLabors.ImageSharp.Image;

namespace BreganTwitchBot.Core.DiscordBot.Commands.Modules.Levelling
{
    internal class DiscordLevelling
    {
        private const long baseXp = 10;

        public static async Task AddDiscordXp(ulong discordUserId, int discordXp, ulong discordChannel)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.DiscordUserId == discordUserId).FirstOrDefault();

                if (user == null)
                {
                    return;
                }

                user.DiscordXp += discordXp;
                context.Users.Update(user);
                context.SaveChanges();
            }

            await Task.Run(async () => await CheckIfLevelledUp(discordUserId, discordChannel));
            Log.Information($"[Discord Levelling] {discordXp}xp added to {discordUserId}");
        }

        public static async Task HandleLevelCommand(SocketInteractionContext ctx, IUser discordUser)
        {
            long currentUserXp;
            long level;

            if (discordUser != null)
            {
                currentUserXp = GetUserXp(discordUser.Id);
                level = GetUserLevel(discordUser.Id);
            }
            else
            {
                currentUserXp = GetUserXp(ctx.User.Id);
                level = GetUserLevel(ctx.User.Id);
            }

            long xpNeededForLevelUp;
            long xpNeededForLastLevelUp;
            long lastLevelXp;
            if (currentUserXp == -1 || level == -1) //if either values don't exist then its set to -1
            {
                var embed = new EmbedBuilder()
                {
                    Title = $"Discord Level error",
                    Description = "dunno don't think it exists lol"
                };

                await ctx.Interaction.FollowupAsync(embed: embed.Build());
            }

            switch (level)
            {
                case 0:
                    lastLevelXp = 0;
                    xpNeededForLevelUp = 5;
                    xpNeededForLastLevelUp = 0;
                    break;
                case 1:
                    lastLevelXp = 5;
                    xpNeededForLevelUp = 10;
                    xpNeededForLastLevelUp = 5;
                    break;
                default:
                    lastLevelXp = baseXp * (level - 1);
                    xpNeededForLevelUp = (long)Math.Round((lastLevelXp * 1.08) * level);
                    xpNeededForLastLevelUp = (long)Math.Round(((baseXp * (level - 2)) * 1.08) * (level - 1));
                    break;
            }

            //Batku in general chat saved the day as I don't know percentages 
            var xpDiff = xpNeededForLevelUp - xpNeededForLastLevelUp;
            var percentageTillNextLevel = Math.Round(((double)(currentUserXp - xpNeededForLastLevelUp) / xpDiff) * 100, 2);

            //Make the image
            var imageName = EditImage((int)Math.Ceiling(percentageTillNextLevel * 4));

            //Build the embed
            if (discordUser != null)
            {
                var embed = new EmbedBuilder()
                {
                    Title = $"Discord Level - {discordUser.Username}",
                    Color = new Color(0, 217, 255),
                    ImageUrl = $"attachment://{imageName}"
                };

                embed.WithThumbnailUrl(discordUser.GetAvatarUrl());

                embed.AddField("Current Level:", $"{level} ({currentUserXp:N0}xp)", true);
                embed.AddField("Next Level:", $"{level + 1} ({xpNeededForLevelUp:N0}xp)", true);
                embed.AddField("Xp till next level:", $"{xpNeededForLevelUp - currentUserXp:N0}xp ({percentageTillNextLevel}%)", true);

                using (var fs = new FileStream(imageName, FileMode.Open))
                {
                    await ctx.Interaction.FollowupWithFileAsync(fs, imageName, embed: embed.Build());
                }
            }
            else
            {
                var embed = new EmbedBuilder()
                {
                    Title = $"Discord Level - {ctx.User.Username}",
                    Color = new Color(0, 217, 255),
                    ImageUrl = $"attachment://{imageName}"
                };

                embed.WithThumbnailUrl(ctx.User.GetAvatarUrl());

                embed.AddField("Current Level:", $"{level} ({currentUserXp:N0}xp)", true);
                embed.AddField("Next Level:", $"{level + 1} ({xpNeededForLevelUp:N0}xp)", true);
                embed.AddField("Xp till next level:", $"{xpNeededForLevelUp - currentUserXp:N0}xp ({percentageTillNextLevel}%)", true);

                using (var fs = new FileStream(imageName, FileMode.Open))
                {
                    await ctx.Interaction.FollowupWithFileAsync(fs, imageName, embed: embed.Build());
                }
            }
        }

        public static async Task HandleToggleLevelUpCommand(SocketInteractionContext ctx)
        {
            using (var context = new DatabaseContext())
            {
                var isToggled = context.Users.Where(x => x.DiscordUserId == ctx.User.Id).First().DiscordLevelUpNotifsEnabled;

                if (isToggled)
                {
                    context.Users.Where(x => x.DiscordUserId == ctx.User.Id).First().DiscordLevelUpNotifsEnabled = false;
                    await ctx.Interaction.RespondAsync($"{ctx.User.Mention} => Notifications have been disabled!");
                }
                else
                {
                    context.Users.Where(x => x.DiscordUserId == ctx.User.Id).First().DiscordLevelUpNotifsEnabled = true;
                    await ctx.Interaction.RespondAsync($"{ctx.User.Mention} => Notifications have been enabled!");
                }

                context.SaveChanges();
            }
        }

        private static async Task CheckIfLevelledUp(ulong discordUserId, ulong discordChannel)
        {
            var xp = GetUserXp(discordUserId);
            var level = GetUserLevel(discordUserId);
            long xpNeededForLevelUp;
            bool userLevelledUp = false;

            if (xp == -1 || level == -1) //if either values don't exist then its set to -1
            {
                return;
            }

            //Check if they have levelled up - could be mutliple to slap it in a while loop
            while (true)
            {
                //work out how much xp for the next level
                switch (level)
                {
                    case 0:
                        xpNeededForLevelUp = 5;
                        break;
                    case 1:
                        xpNeededForLevelUp = 10;
                        break;
                    default:
                        var lastLevelXp = baseXp * (level - 1);
                        xpNeededForLevelUp = (long)Math.Round((lastLevelXp * 1.08) * level);
                        break;
                }

                if (xp >= xpNeededForLevelUp)
                {
                    level++;

                    using (var context = new DatabaseContext())
                    {
                        context.Users.Where(x => x.DiscordUserId == discordUserId).FirstOrDefault().DiscordLevel++;
                        context.SaveChanges();
                    }

                    DiscordHelper.AddUserPoints(discordUserId, level * 2000);
                    Log.Information($"[Discord Levelling] {discordUserId} has levelled up to {level}");
                    userLevelledUp = true;
                    continue;
                }

                break;
            }

            Log.Information("[Discord Levelling] Levelling done");

            bool notifsEnabled;

            using (var context = new DatabaseContext())
            {
                notifsEnabled = context.Users.Where(x => x.DiscordLevelUpNotifsEnabled).First().DiscordLevelUpNotifsEnabled;
            }

            if (notifsEnabled && userLevelledUp)
            {
                Log.Information("[Discord Levelling] Sending message");
                if (level == 1 || level == 2)
                {
                    return;
                }

                await DiscordHelper.SendMessage(discordChannel, $"**GG** <@{discordUserId}> you have levelled up to level **{level}**! You have gained **{level * 2000:N0}** {Config.PointsName}! (you can disable level up messages by doing /togglelevelups in <#{Config.DiscordCommandsChannelID}>)");
            }
        }

        private static string EditImage(int percentage)
        {
            var random = new Random();
            var backgroundToUse = "";
            switch (random.Next(1, 8))
            {
                case 1:
                    backgroundToUse = "blocks4Head";
                    break;
                case 2:
                    backgroundToUse = "blocksAngry";
                    break;
                case 3:
                    backgroundToUse = "blocksGuineaG";
                    break;
                case 4:
                    backgroundToUse = "Guinea";
                    break;
                case 5:
                    backgroundToUse = "KEKW";
                    break;
                case 6:
                    backgroundToUse = "shrekW";
                    break;
                case 7:
                    backgroundToUse = "widePeepoHappy";
                    break;
            }
            //Check if the File exits
            if (File.Exists($"{backgroundToUse}Bar.png"))
            {
                File.Delete($"{backgroundToUse}Bar.png");
            }

            //Setup the bitmap
            using (var blankBackground = Image.Load("Skins/blankProgress.png"))
            {
                //Load the image and cut it
                var cutImage = CutImage(Image.Load($"ProgressBars/{backgroundToUse}.png"), percentage);

                //Draw the rectangle for the background
                blankBackground.Mutate(x => x.DrawImage(cutImage, new Point(0, 0), 1));

                try
                {
                    blankBackground.SaveAsPng($"{backgroundToUse}Bar.png");
                    blankBackground.Dispose();
                }
                catch (Exception e)
                {
                    Log.Error($"[Discord Levelling] Error saving image - {e}");
                }

                return $"{backgroundToUse}Bar.png";
            }
        }

        private static Image CutImage(Image bmp, int amountToCutOff)
        {
            var rect = new Rectangle(amountToCutOff, 0, 400, 50);
            //var rect = new RectangleF(0, 0, amountToCutOff, 50);
            bmp.Mutate(x => x.Clear(SixLabors.ImageSharp.Color.FromRgba(0, 0, 0, 0), rect));
            bmp.SaveAsPng("pootest.png");
            return bmp;
        }

        private static long GetUserXp(ulong userId)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.DiscordUserId == userId).FirstOrDefault();

                if (user == null)
                {
                    return 0;
                }

                return user.DiscordXp;
            }
        }

        private static long GetUserLevel(ulong userId)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Where(x => x.DiscordUserId == userId).FirstOrDefault();

                if (user == null)
                {
                    return 0;
                }

                return user.DiscordLevel;
            }
        }
    }
}
