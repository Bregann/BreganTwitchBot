namespace BreganUtils
{
    public class DateTimeHelper
    {
        public static DateTime ConvertDateTimeToLocalTime(string timezoneId, DateTime dt)
        {
            var destinazionTZ = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);

            return TimeZoneInfo.ConvertTimeFromUtc(dt, destinazionTZ);
        }

        public static string HumanizeDateTime(DateTime dt)
        {
            return dt.ToString("dddd") + " " + dt.ToString("dd").Replace("0", "") + GetDaySuffix(dt.Day) + " " + dt.ToString("MMMM") + " " + dt.ToString("yyyy");
        }

        public static string? HumanizeDateTimeWithTime(DateTimeOffset? dt)
        {
            if (dt == null)
            {
                return null;
            }
            return dt.Value.ToString("dddd") + " " + dt.Value.ToString("dd").Replace("0", "") + GetDaySuffix(dt.Value.Day) + " " + dt.Value.ToString("MMMM") + " " + dt.Value.ToString("yyyy") + " @ " + dt.Value.ToString("HH:mm:ss zzz");
        }



        private static string GetDaySuffix(int day)
        {
            switch (day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";

                case 2:
                case 22:
                    return "nd";

                case 3:
                case 23:
                    return "rd";

                default:
                    return "th";
            }
        }
    }
}