using RestSharp;
using Serilog;

namespace BreganUtils.ProjectMonitor.Projects
{
    public class ProjectMonitorBreganTwitchBot
    {
        public static void SendUsersInStreamUpdate(long users)
        {
            try
            {
                if (ProjectMonitorConfig.ApiKey == "")
                {
                    return;
                }

                var client = new RestClient(ProjectMonitorConfig.ApiUrl);
                var request = new RestRequest("/api/BreganTwitchBot/UpdateUsersInStream", Method.Post);

                request.AddHeader("Authorization", ProjectMonitorConfig.ApiKey);
                request.AddHeader("EnvMode", ProjectMonitorConfig.Mode);

                request.AddBody(users);
                client.Post(request);
            }
            catch (Exception e)
            {
                Log.Error($"[Project Monitor] Error with SendUsersInStreamUpdate - {e}");
                return;
            }
        }

        public static void SendTwitchIRCConnectionStateUpdate(bool status)
        {
            try
            {
                if (ProjectMonitorConfig.ApiKey == "")
                {
                    return;
                }

                var client = new RestClient(ProjectMonitorConfig.ApiUrl);
                var request = new RestRequest("/api/BreganTwitchBot/UpdateTwitchIRCConnectionState", Method.Post);

                request.AddHeader("Authorization", ProjectMonitorConfig.ApiKey);
                request.AddHeader("EnvMode", ProjectMonitorConfig.Mode);

                request.AddBody(status);
                client.Post(request);
            }
            catch (Exception e)
            {
                Log.Error($"[Project Monitor] Error with SendTwitchIRCConnectionStateUpdate - {e}");
                return;
            }
        }

        public static void SendTwitchPubSubConnectionStateUpdate(bool status)
        {
            try
            {
                if (ProjectMonitorConfig.ApiKey == "")
                {
                    return;
                }

                var client = new RestClient(ProjectMonitorConfig.ApiUrl);
                var request = new RestRequest("/api/BreganTwitchBot/UpdateTwitchPubSubConnectionState", Method.Post);

                request.AddHeader("Authorization", ProjectMonitorConfig.ApiKey);
                request.AddHeader("EnvMode", ProjectMonitorConfig.Mode);

                request.AddBody(status);
                client.Post(request);
            }
            catch (Exception e)
            {
                Log.Error($"[Project Monitor] Error with SendTwitchPubSubConnectionStateUpdate - {e}");
                return;
            }
        }

        public static void SendTwitchAPIKeyRefreshUpdate()
        {
            try
            {
                if (ProjectMonitorConfig.ApiKey == "")
                {
                    return;
                }

                var client = new RestClient(ProjectMonitorConfig.ApiUrl);
                var request = new RestRequest("/api/BreganTwitchBot/UpdateTwitchAPIKeyRefreshTime", Method.Post);

                request.AddHeader("Authorization", ProjectMonitorConfig.ApiKey);
                request.AddHeader("EnvMode", ProjectMonitorConfig.Mode);

                client.Post(request);
            }
            catch (Exception e)
            {
                Log.Error($"[Project Monitor] Error with SendTwitchAPIKeyRefreshUpdate - {e}");
                return;
            }
        }

        public static void SendDiscordConnectionStateUpdate(bool status)
        {
            try
            {
                if (ProjectMonitorConfig.ApiKey == "")
                {
                    return;
                }

                var client = new RestClient(ProjectMonitorConfig.ApiUrl);
                var request = new RestRequest("/api/BreganTwitchBot/UpdateDiscordBotConnectionState", Method.Post);

                request.AddHeader("Authorization", ProjectMonitorConfig.ApiKey);
                request.AddHeader("EnvMode", ProjectMonitorConfig.Mode);

                request.AddBody(status);
                client.Post(request);
            }
            catch (Exception e)
            {
                Log.Error($"[Project Monitor] Error with SendDiscordConnectionStateUpdate - {e}");
                return;
            }
        }

        public static void SendStreamAnnouncedUpdate(bool status)
        {
            try
            {
                if (ProjectMonitorConfig.ApiKey == "")
                {
                    return;
                }

                var client = new RestClient(ProjectMonitorConfig.ApiUrl);
                var request = new RestRequest("/api/BreganTwitchBot/UpdateStreamAnnounced", Method.Post);

                request.AddHeader("Authorization", ProjectMonitorConfig.ApiKey);
                request.AddHeader("EnvMode", ProjectMonitorConfig.Mode);

                request.AddBody(status);
                client.Post(request);
            }
            catch (Exception e)
            {
                Log.Error($"[Project Monitor] Error with SendStreamAnnouncedUpdate - {e}");
                return;
            }
        }

        public static void SendStreamStatusUpdate(bool status)
        {
            try
            {
                if (ProjectMonitorConfig.ApiKey == "")
                {
                    return;
                }

                var client = new RestClient(ProjectMonitorConfig.ApiUrl);
                var request = new RestRequest("/api/BreganTwitchBot/UpdateStreamStatus", Method.Post);

                request.AddHeader("Authorization", ProjectMonitorConfig.ApiKey);
                request.AddHeader("EnvMode", ProjectMonitorConfig.Mode);

                request.AddBody(status);
                client.Post(request);
            }
            catch (Exception e)
            {
                Log.Error($"[Project Monitor] Error with SendStreamStatusUpdate - {e}");
                return;
            }
        }

        public static void SendStreamUptimeUpdate(TimeSpan time)
        {
            try
            {
                if (ProjectMonitorConfig.ApiKey == "")
                {
                    return;
                }

                var client = new RestClient(ProjectMonitorConfig.ApiUrl);
                var request = new RestRequest("/api/BreganTwitchBot/UpdateStreamUptime", Method.Post);

                request.AddHeader("Authorization", ProjectMonitorConfig.ApiKey);
                request.AddHeader("EnvMode", ProjectMonitorConfig.Mode);

                request.AddBody(time);
                client.Post(request);
            }
            catch (Exception e)
            {
                Log.Error($"[Project Monitor] Error with SendStreamUptimeUpdate - {e}");
                return;
            }
        }

        public static void SendDiscordDailyPointsCollectingUpdate(bool status)
        {
            try
            {
                if (ProjectMonitorConfig.ApiKey == "")
                {
                    return;
                }

                var client = new RestClient(ProjectMonitorConfig.ApiUrl);
                var request = new RestRequest("/api/BreganTwitchBot/UpdateTwitchDailyPointsCollectingStatus", Method.Post);

                request.AddHeader("Authorization", ProjectMonitorConfig.ApiKey);
                request.AddHeader("EnvMode", ProjectMonitorConfig.Mode);

                request.AddBody(status);
                client.Post(request);
            }
            catch (Exception e)
            {
                Log.Error($"[Project Monitor] Error with SendDiscordDailyPointsCollectingUpdate - {e}");
                return;
            }
        }

        public static void SendLastHoursUpdateUpdate()
        {
            try
            {
                if (ProjectMonitorConfig.ApiKey == "")
                {
                    return;
                }

                var client = new RestClient(ProjectMonitorConfig.ApiUrl);
                var request = new RestRequest("/api/BreganTwitchBot/UpdateLastHoursUpdateTime", Method.Post);

                request.AddHeader("Authorization", ProjectMonitorConfig.ApiKey);
                request.AddHeader("EnvMode", ProjectMonitorConfig.Mode);

                client.Post(request);
            }
            catch (Exception e)
            {
                Log.Error($"[Project Monitor] Error with SendLastHoursUpdateUpdate - {e}");
                return;
            }
        }

        public static void SendDiscordLeaderboardUpdate()
        {
            try
            {
                if (ProjectMonitorConfig.ApiKey == "")
                {
                    return;
                }

                var client = new RestClient(ProjectMonitorConfig.ApiUrl);
                var request = new RestRequest("/api/BreganTwitchBot/UpdateLastDiscordLeaderboardUpdate", Method.Post);

                request.AddHeader("Authorization", ProjectMonitorConfig.ApiKey);
                request.AddHeader("EnvMode", ProjectMonitorConfig.Mode);

                client.Post(request);
            }
            catch (Exception e)
            {
                Log.Error($"[Project Monitor] Error with SendDiscordLeaderboardUpdate - {e}");
                return;
            }
        }
    }
}