using BreganUtils.Dtos;
using RestSharp;
using Serilog;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

namespace BreganUtils.ProjectMonitor
{
    public class ProjectMonitorCommon
    {
        private static Timer _timer1;

        public static void ReportProjectUp(string projectName)
        {
            if (ProjectMonitorConfig.ApiKey == "" || ProjectMonitorConfig.Mode == "debug")
            {
                return;
            }

            try
            {
                //Ping the api every 5s reporting that the service is up
                _timer1 = new Timer(10000);
                _timer1.Elapsed += (sender, e) => SendProjectApiRequestUptime(sender, e, projectName);
                _timer1.Enabled = true;
                _timer1.Start();
            }
            catch (Exception e)
            {
                Log.Error($"[Project Monitor] Error with - ReportProjectUp {e}");
                return;
            }
        }

        private static async void SendProjectApiRequestUptime(object sender, ElapsedEventArgs e, string projectName)
        {
            try
            {
                //Get the process data
                var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
                var cpuUsage = await GetCpuUsageOfApplication();
                var ramUsage = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);

                var client = new RestClient(ProjectMonitorConfig.ApiUrl);
                var request = new RestRequest("/api/CommonUpdates/AddAndUpdateProjectHealthStatus", Method.Post);

                request.AddHeader("Authorization", ProjectMonitorConfig.ApiKey);

                request.AddBody(new AddAndUpdateProjectHealthStatusDto
                {
                    ProjectName = projectName,
                    ProjectUptime = uptime,
                    CPUUsage = cpuUsage,
                    RAMUsage = ramUsage
                });

                client.Post(request);
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Monitor] Error with SendProjectApiRequestUptime - {ex}");
                return;
            }
        }

        private static async Task<double> GetCpuUsageOfApplication()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            await Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
        }
    }
}