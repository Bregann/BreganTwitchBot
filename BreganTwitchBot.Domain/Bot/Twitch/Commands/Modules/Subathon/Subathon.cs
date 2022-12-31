using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Domain.Bot.Twitch.Helpers;
using Humanizer;
using Humanizer.Localisation;
using Serilog;

namespace BreganTwitchBot.Domain.Bot.Twitch.Commands.Modules.Subathon
{
    public class Subathon
    {
        private static DateTime _subathonCooldown;

        public static void AddSubathonSubTime(TwitchLib.Client.Enums.SubscriptionPlan subType, string gifterUsername)
        {
            if (!AppConfig.SubathonActive)
            {
                return;
            }

            AddOrUpdateUserToSubathonTable(gifterUsername, 1, "sub");
            Log.Information($"[Subathon Time] {AppConfig.SubathonTime}");
            Log.Information($"[Subathon Time] {AppConfig.SubathonTime.TotalHours}");
            Log.Information($"[Subathon Time] {Math.Floor(AppConfig.SubathonTime.TotalHours)}");

            switch (Math.Floor(AppConfig.SubathonTime.TotalHours))
            {
                case <= 11:
                    switch (subType)
                    {
                        case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                            AppConfig.AddSubathonTime(TimeSpan.FromMinutes(6));
                            Log.Information("[Subathon Time] Subathon time increased by 6 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            AppConfig.AddSubathonTime(TimeSpan.FromMinutes(12));
                            Log.Information("[Subathon Time] Subathon time increased by 12 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            AppConfig.AddSubathonTime(TimeSpan.FromMinutes(30));
                            Log.Information("[Subathon Time] Subathon time increased by 30 minutes");
                            break;
                    }
                    break;

                case 12:
                    switch (subType)
                    {
                        case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                            AppConfig.AddSubathonTime(TimeSpan.FromMinutes(3));
                            Log.Information("[Subathon Time] Subathon time increased by 3 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            AppConfig.AddSubathonTime(TimeSpan.FromMinutes(6));
                            Log.Information("[Subathon Time] Subathon time increased by 6 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            AppConfig.AddSubathonTime(TimeSpan.FromMinutes(15));
                            Log.Information("[Subathon Time] Subathon time increased by 15 minutes");
                            break;
                    }
                    break;

                case 13:
                    switch (subType)
                    {
                        case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                            AppConfig.AddSubathonTime(TimeSpan.FromMinutes(2));
                            Log.Information("[Subathon Time] Subathon time increased by 2 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            AppConfig.AddSubathonTime(TimeSpan.FromMinutes(4));
                            Log.Information("[Subathon Time] Subathon time increased by 4 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            AppConfig.AddSubathonTime(TimeSpan.FromMinutes(10));
                            Log.Information("[Subathon Time] Subathon time increased by 10 minutes");
                            break;
                    }
                    break;

                case 14:
                    switch (subType)
                    {
                        case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                            AppConfig.AddSubathonTime(TimeSpan.FromMinutes(1));
                            Log.Information("[Subathon Time] Subathon time increased by 1 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            AppConfig.AddSubathonTime(TimeSpan.FromMinutes(2));
                            Log.Information("[Subathon Time] Subathon time increased by 2 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            AppConfig.AddSubathonTime(TimeSpan.FromMinutes(5));
                            Log.Information("[Subathon Time] Subathon time increased by 5 minutes");
                            break;
                    }
                    break;

                case 15:
                    switch (subType)
                    {
                        case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                            AppConfig.AddSubathonTime(TimeSpan.FromSeconds(30));
                            Log.Information("[Subathon Time] Subathon time increased by 30 seconds");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            AppConfig.AddSubathonTime(TimeSpan.FromMinutes(1));
                            Log.Information("[Subathon Time] Subathon time increased by 1 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            AppConfig.AddSubathonTime(TimeSpan.FromSeconds(170));
                            Log.Information("[Subathon Time] Subathon time increased by 2 1/2 minutes");
                            break;
                    }
                    break;

                default: //16h+
                    Log.Information($"[Subathon Time] {AppConfig.SubathonTime}");
                    switch (subType)
                    {
                        case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                            AppConfig.AddSubathonTime(TimeSpan.FromSeconds(10));
                            Log.Information("[Subathon Time] Subathon time increased by 10 seconds");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            AppConfig.AddSubathonTime(TimeSpan.FromSeconds(20));
                            Log.Information("[Subathon Time] Subathon time increased by 20 seconds");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            AppConfig.AddSubathonTime(TimeSpan.FromSeconds(50));
                            Log.Information("[Subathon Time] Subathon time increased by 50 seconds");
                            break;
                    }
                    break;
            }
        }

        public static void AddSubathonBitsTime(int bitsAmount, string cheererUsername)
        {
            if (!AppConfig.SubathonActive)
            {
                return;
            }

            AddOrUpdateUserToSubathonTable(cheererUsername, bitsAmount, "bits");

            //Work out to see if it needs to loop or not
            var subTimeHoursConfig = AppConfig.SubathonTime;
            Log.Information($"[Subathon Time] {AppConfig.SubathonTime}");
            Log.Information($"[Subathon Time] {AppConfig.SubathonTime.TotalHours}");
            Log.Information($"[Subathon Time] {Math.Floor(AppConfig.SubathonTime.TotalHours)}");

            switch (Math.Floor(AppConfig.SubathonTime.TotalHours))
            {
                case <= 11:
                    subTimeHoursConfig += TimeSpan.FromMilliseconds(900 * bitsAmount);
                    break;

                case 12:
                    subTimeHoursConfig += TimeSpan.FromMilliseconds(450 * bitsAmount);
                    break;

                case 13:
                    subTimeHoursConfig += TimeSpan.FromMilliseconds(220 * bitsAmount);
                    break;

                case 14:
                    subTimeHoursConfig += TimeSpan.FromMilliseconds(110 * bitsAmount);
                    break;

                case 15:
                    subTimeHoursConfig += TimeSpan.FromMilliseconds(50 * bitsAmount);
                    break;

                default: //16h+
                    subTimeHoursConfig += TimeSpan.FromMilliseconds(20 * bitsAmount);
                    break;
            }

            //See if the updated time changes hours - if not then save and return
            if (Math.Floor(subTimeHoursConfig.TotalHours) == Math.Floor(AppConfig.SubathonTime.TotalHours))
            {
                switch (Math.Floor(AppConfig.SubathonTime.TotalHours))
                {
                    case <= 11:
                        AppConfig.AddSubathonTime(TimeSpan.FromMilliseconds(900 * bitsAmount));
                        break;

                    case 12:
                        AppConfig.AddSubathonTime(TimeSpan.FromMilliseconds(450 * bitsAmount));
                        break;

                    case 13:
                        AppConfig.AddSubathonTime(TimeSpan.FromMilliseconds(220 * bitsAmount));
                        break;

                    case 14:
                        AppConfig.AddSubathonTime(TimeSpan.FromMilliseconds(110 * bitsAmount));
                        break;

                    case 15:
                        AppConfig.AddSubathonTime(TimeSpan.FromMilliseconds(50 * bitsAmount));
                        break;

                    default: //16h+
                        AppConfig.AddSubathonTime(TimeSpan.FromMilliseconds(20 * bitsAmount));
                        break;
                }

                return;
            }

            //loop through every bit to add the correct time

            var totalTsToAdd = TimeSpan.FromSeconds(0);

            for (int i = 0; i < bitsAmount; i++)
            {
                switch (Math.Floor(AppConfig.SubathonTime.TotalHours))
                {
                    case <= 11:
                        totalTsToAdd += TimeSpan.FromMilliseconds(900);
                        break;

                    case 12:
                        totalTsToAdd += TimeSpan.FromMilliseconds(450);
                        break;

                    case 13:
                        totalTsToAdd += TimeSpan.FromMilliseconds(220);
                        break;

                    case 14:
                        totalTsToAdd += TimeSpan.FromMilliseconds(110);
                        break;

                    case 15:
                        totalTsToAdd += TimeSpan.FromMilliseconds(50);
                        break;

                    default: //16h+
                        totalTsToAdd += TimeSpan.FromMilliseconds(20);
                        break;
                }
            }

            Log.Information($"[Subathon Time] Subathon time to add {totalTsToAdd}");
            AppConfig.AddSubathonTime(totalTsToAdd);
        }

        public static void HandleSubathonCommand(string username)
        {
            //5 sec cooldown to prevent spam
            if (DateTime.UtcNow - TimeSpan.FromSeconds(5) <= _subathonCooldown && !SuperMods.Supermods.IsUserSupermod(username))
            {
                return;
            }

            if (!AppConfig.SubathonActive)
            {
                return;
            }

            //Get the stream uptime
            var startTime = new DateTime(2022, 9, 25, 11, 35, 0);

            var endTimeDT = startTime.Add(AppConfig.SubathonTime);
            var timeLeft = endTimeDT - DateTime.UtcNow;

            TwitchHelper.SendMessage($"@{username} => The subathon has been extended to a total of {AppConfig.SubathonTime.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)}! The stream will end in {timeLeft.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)}. See all the info at https://bot.bregan.me/subathon");
            _subathonCooldown = DateTime.UtcNow;
        }

        private static void AddOrUpdateUserToSubathonTable(string username, int amount, string type)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Subathon.Where(x => x.Username == username).FirstOrDefault();

                if (user == null)
                {
                    if (type == "sub")
                    {
                        context.Subathon.Add(new BreganTwitchBot.Infrastructure.Database.Models.Subathon
                        {
                            Username = username,
                            BitsDonated = 0,
                            SubsGifted = amount
                        });
                    }
                    else
                    {
                        context.Subathon.Add(new BreganTwitchBot.Infrastructure.Database.Models.Subathon
                        {
                            Username = username,
                            BitsDonated = amount,
                            SubsGifted = 0
                        });
                    }
                }
                else
                {
                    if (type == "sub")
                    {
                        user.SubsGifted += amount;
                    }
                    else
                    {
                        user.BitsDonated += amount;
                    }
                }

                context.SaveChanges();
                Log.Information($"[Subathon] {amount} added to {username} type {type}");
            }
        }
    }
}