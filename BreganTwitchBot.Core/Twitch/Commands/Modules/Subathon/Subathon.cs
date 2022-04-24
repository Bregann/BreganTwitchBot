using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Core.Twitch.Commands.Modules.Subathon
{
    /*
    public class Subathon
    {
        private static DateTime _subathonCooldown;

        public static void AddSubathonSubTime(TwitchLib.Client.Enums.SubscriptionPlan subType)
        {
            if (Config.SubathonActive == "false")
            {
                return;
            }

            Log.Information($"[Subathon Time] {Config.SubathonTime}");
            Log.Information($"[Subathon Time] {Config.SubathonTime.TotalHours}");
            Log.Information($"[Subathon Time] {Math.Floor(Config.SubathonTime.TotalHours)}");

            switch (Math.Floor(Config.SubathonTime.TotalHours))
            {
                case <= 11:
                    switch (subType)
                    {
                        case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                            Config.SubathonTime += TimeSpan.FromMinutes(6);
                            Log.Information("[Subathon Time] Subathon time increased by 6 minutes");
                            break;
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            Config.SubathonTime += TimeSpan.FromMinutes(12);
                            Log.Information("[Subathon Time] Subathon time increased by 12 minutes");
                            break;
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            Config.SubathonTime += TimeSpan.FromMinutes(30);
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
                            Config.SubathonTime += TimeSpan.FromMinutes(3);
                            Log.Information("[Subathon Time] Subathon time increased by 3 minutes");
                            break;
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            Config.SubathonTime += TimeSpan.FromMinutes(6);
                            Log.Information("[Subathon Time] Subathon time increased by 6 minutes");
                            break;
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            Config.SubathonTime += TimeSpan.FromMinutes(15);
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
                            Config.SubathonTime += TimeSpan.FromMinutes(2);
                            Log.Information("[Subathon Time] Subathon time increased by 2 minutes");
                            break;
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            Config.SubathonTime += TimeSpan.FromMinutes(4);
                            Log.Information("[Subathon Time] Subathon time increased by 4 minutes");
                            break;
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            Config.SubathonTime += TimeSpan.FromMinutes(10);
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
                            Config.SubathonTime += TimeSpan.FromMinutes(1);
                            Log.Information("[Subathon Time] Subathon time increased by 1 minutes");
                            break;
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            Config.SubathonTime += TimeSpan.FromMinutes(2);
                            Log.Information("[Subathon Time] Subathon time increased by 2 minutes");
                            break;
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            Config.SubathonTime += TimeSpan.FromMinutes(5);
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
                            Config.SubathonTime += TimeSpan.FromSeconds(30);
                            Log.Information("[Subathon Time] Subathon time increased by 30 seconds");
                            break;
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            Config.SubathonTime += TimeSpan.FromMinutes(1);
                            Log.Information("[Subathon Time] Subathon time increased by 1 minutes");
                            break;
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            Config.SubathonTime += TimeSpan.FromSeconds(170);
                            Log.Information("[Subathon Time] Subathon time increased by 2 1/2 minutes");
                            break;
                    }
                    break;
                default: //16h+
                    Log.Information($"[Subathon Time] {Config.SubathonTime}");
                    switch (subType)
                    {
                        case TwitchLib.Client.Enums.SubscriptionPlan.NotSet:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Prime:
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier1:
                            Config.SubathonTime += TimeSpan.FromSeconds(10);
                            Log.Information("[Subathon Time] Subathon time increased by 10 seconds");
                            break;
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier2:
                            Config.SubathonTime += TimeSpan.FromSeconds(20);
                            Log.Information("[Subathon Time] Subathon time increased by 20 seconds");
                            break;
                        case TwitchLib.Client.Enums.SubscriptionPlan.Tier3:
                            Config.SubathonTime += TimeSpan.FromSeconds(50);
                            Log.Information("[Subathon Time] Subathon time increased by 50 seconds");
                            break;
                    }
                    break;
            }

            //Save to config
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["subTime"].Value = Config.SubathonTime.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            Log.Information("[Subathon Time] Subathon config time saved");
        }

        public static void AddSubathonBitsTime(int bitsAmount)
        {
            if (Config.SubathonActive == "false")
            {
                return;
            }

            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //Work out to see if it needs to loop or not
            var subTimeHoursConfig = Config.SubathonTime;
            Log.Information($"[Subathon Time] {Config.SubathonTime}");
            Log.Information($"[Subathon Time] {Config.SubathonTime.TotalHours}");
            Log.Information($"[Subathon Time] {Math.Floor(Config.SubathonTime.TotalHours)}");

            switch (Math.Floor(Config.SubathonTime.TotalHours))
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
            if (Math.Floor(subTimeHoursConfig.TotalHours) == Math.Floor(Config.SubathonTime.TotalHours))
            {
                switch (Math.Floor(Config.SubathonTime.TotalHours))
                {
                    case <= 11:
                        Config.SubathonTime += TimeSpan.FromMilliseconds(900 * bitsAmount);
                        break;
                    case 12:
                        Config.SubathonTime += TimeSpan.FromMilliseconds(450 * bitsAmount);
                        break;
                    case 13:
                        Config.SubathonTime += TimeSpan.FromMilliseconds(220 * bitsAmount);
                        break;
                    case 14:
                        Config.SubathonTime += TimeSpan.FromMilliseconds(110 * bitsAmount);
                        break;
                    case 15:
                        Config.SubathonTime += TimeSpan.FromMilliseconds(50 * bitsAmount);
                        break;
                    default: //16h+
                        Config.SubathonTime += TimeSpan.FromMilliseconds(20 * bitsAmount);
                        break;
                }

                //Save to config
                config.AppSettings.Settings["subTime"].Value = Config.SubathonTime.ToString();
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                Log.Information("[Subathon Time] Subathon config time saved");
                return;
            }

            //loop through every bit to add the correct time
            for (int i = 0; i < bitsAmount; i++)
            {
                switch (Math.Floor(Config.SubathonTime.TotalHours))
                {
                    case <= 11:
                        Config.SubathonTime += TimeSpan.FromMilliseconds(900);
                        break;
                    case 12:
                        Config.SubathonTime += TimeSpan.FromMilliseconds(450);
                        break;
                    case 13:
                        Config.SubathonTime += TimeSpan.FromMilliseconds(220);
                        break;
                    case 14:
                        Config.SubathonTime += TimeSpan.FromMilliseconds(110);
                        break;
                    case 15:
                        Config.SubathonTime += TimeSpan.FromMilliseconds(50);
                        break;
                    default: //16h+
                        Config.SubathonTime += TimeSpan.FromMilliseconds(20);
                        break;
                }
            }

            //Save to config
            config.AppSettings.Settings["subTime"].Value = Config.SubathonTime.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            Log.Information("[Subathon Time] Subathon config time saved");
        }

        public static async Task HandleSubathonCommand(string username)
        {
            //5 sec cooldown to prevent spam
            if (DateTime.Now - TimeSpan.FromSeconds(5) <= _subathonCooldown && !Supermods.IsSupermod(username))
            {
                return;
            }

            if (Config.SubathonActive == "false")
            {
                return;
            }

            //Get the stream uptime
            var startTime = new DateTime(2021, 9, 12, 11, 5, 0);

            var endTimeDT = startTime.Add(Config.SubathonTime);
            var timeLeft = endTimeDT - DateTime.Now;

            TwitchBotConnection.SendMessage($"@{username} => The subathon has been extended to a total of {Config.SubathonTime.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)}! The stream will end in {timeLeft.Humanize(maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second, precision: 7)}");
            _subathonCooldown = DateTime.Now;
        }
    }*/
}
