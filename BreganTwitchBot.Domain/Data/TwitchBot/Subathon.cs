using BreganTwitchBot.Domain.Data.TwitchBot.Helpers;
using BreganTwitchBot.Infrastructure.Database.Context;
using Serilog;

namespace BreganTwitchBot.Domain.Data.TwitchBot
{
    public class Subathon
    {
        public static async Task AddSubathonBitsTime(int bitsAmount, string cheererUsername)
        {
            if (!AppConfig.SubathonActive)
            {
                return;
            }

            AddOrUpdateUserToSubathonTable(cheererUsername, bitsAmount, "bits");

            //Work out to see if it needs to loop or not
            var subTimeHoursConfig = AppConfig.SubathonTime;
            Log.Information($"[Subathon] {AppConfig.SubathonTime}");
            Log.Information($"[Subathon] {AppConfig.SubathonTime.TotalHours}");
            Log.Information($"[Subathon] {Math.Floor(AppConfig.SubathonTime.TotalHours)}");

            switch (Math.Floor(AppConfig.SubathonTime.TotalHours))
            {
                case <= 11:
                    subTimeHoursConfig += TimeSpan.FromMilliseconds(1800 * bitsAmount);
                    break;
                case 12:
                case 13:
                case 14:
                case 15:
                    subTimeHoursConfig += TimeSpan.FromMilliseconds(1200 * bitsAmount);
                    break;
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                    subTimeHoursConfig += TimeSpan.FromMilliseconds(600 * bitsAmount);
                    break;
                default: //24h+
                    subTimeHoursConfig += TimeSpan.FromMilliseconds(0);
                    break;
            }

            //See if the updated time changes hours - if not then save and return
            if (Math.Floor(subTimeHoursConfig.TotalHours) == Math.Floor(AppConfig.SubathonTime.TotalHours))
            {
                switch (Math.Floor(AppConfig.SubathonTime.TotalHours))
                {
                    case <= 11:
                        await AppConfig.AddSubathonTime(TimeSpan.FromMilliseconds(1800 * bitsAmount));
                        break;
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        await AppConfig.AddSubathonTime(TimeSpan.FromMilliseconds(1200 * bitsAmount));
                        break;
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                        await AppConfig.AddSubathonTime(TimeSpan.FromMilliseconds(600 * bitsAmount));
                        break;
                    default: //24h+
                        await AppConfig.AddSubathonTime(TimeSpan.FromMilliseconds(0));
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
                        totalTsToAdd += TimeSpan.FromMilliseconds(1800);
                        break;
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                        totalTsToAdd += TimeSpan.FromMilliseconds(1200);
                        break;
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                        totalTsToAdd += TimeSpan.FromMilliseconds(600);
                        break;
                    default: //24h+
                        totalTsToAdd += TimeSpan.FromMilliseconds(0);
                        break;
                }
            }

            Log.Information($"[Subathon] Subathon time to add {totalTsToAdd}");
            await AppConfig.AddSubathonTime(totalTsToAdd);
        }

        public static async Task AddSubathonSubTime(TwitchLib.Client.Enums.SubscriptionPlan subType, string gifterUsername)
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
                            await AppConfig.AddSubathonTime(TimeSpan.FromMinutes(12));
                            Log.Information("[Subathon Time] Subathon time increased by 12 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            await AppConfig.AddSubathonTime(TimeSpan.FromMinutes(24));
                            Log.Information("[Subathon Time] Subathon time increased by 24 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            await AppConfig.AddSubathonTime(TimeSpan.FromMinutes(60));
                            Log.Information("[Subathon Time] Subathon time increased by 60 minutes");
                            break;
                    }
                    break;
                case 12:
                case 13:
                case 14:
                case 15:
                    switch (subType)
                    {
                        case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                            await AppConfig.AddSubathonTime(TimeSpan.FromMinutes(6));
                            Log.Information("[Subathon Time] Subathon time increased by 6 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            await AppConfig.AddSubathonTime(TimeSpan.FromMinutes(12));
                            Log.Information("[Subathon Time] Subathon time increased by 12 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            await AppConfig.AddSubathonTime(TimeSpan.FromMinutes(30));
                            Log.Information("[Subathon Time] Subathon time increased by 30 minutes");
                            break;
                    }
                    break;
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                    switch (subType)
                    {
                        case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                            await AppConfig.AddSubathonTime(TimeSpan.FromMinutes(3));
                            Log.Information("[Subathon Time] Subathon time increased by 3 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            await AppConfig.AddSubathonTime(TimeSpan.FromMinutes(6));
                            Log.Information("[Subathon Time] Subathon time increased by 6 minutes");
                            break;

                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            await AppConfig.AddSubathonTime(TimeSpan.FromMinutes(15));
                            Log.Information("[Subathon Time] Subathon time increased by 15 minutes");
                            break;
                    }
                    break;
                default: //24h+
                    Log.Information($"[Subathon Time] {AppConfig.SubathonTime} maxed");
                    break;
            }
        }

        private static void AddOrUpdateUserToSubathonTable(string username, int amount, string type)
        {
            try
            {
                using (var context = new DatabaseContext())
                {
                    var user = context.Subathon.Where(x => x.Username == username).FirstOrDefault();

                    if (user == null)
                    {
                        if (type == "sub")
                        {
                            context.Subathon.Add(new Infrastructure.Database.Models.Subathon
                            {
                                Username = username,
                                BitsDonated = 0,
                                SubsGifted = amount
                            });
                        }
                        else
                        {
                            context.Subathon.Add(new Infrastructure.Database.Models.Subathon
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
            catch (Exception ex)
            {
                Log.Fatal($"[Subathon] error adding time {ex}");
                TwitchHelper.SendMessage($"error adding {amount} added to {username} type {type}");
                return;
            }
        }
    }
}