using Serilog;

namespace BreganUtils.ProjectMonitor
{
    public class ProjectMonitorConfig
    {
        public static readonly string ApiUrl = "https://dashboardapi.bregan.me";

        public static string Mode { get; private set; } = "";
        public static string ApiKey { get; private set; } = "";

        public static void SetupMonitor(string envMode, string apiKey = "")
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Async(x => x.File("Logs/log.log", retainedFileCountLimit: null, rollingInterval: RollingInterval.Day)).WriteTo.Console().CreateLogger();

            if (envMode != "debug" && envMode != "release")
            {
                throw new ApplicationException("Invalid mode! This should be debug or release");
            }

            Mode = envMode;
            ApiKey = apiKey;

            Log.Information($"[Project Monitor] Config setup - {Mode}");
        }
    }
}