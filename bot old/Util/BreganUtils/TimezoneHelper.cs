using System;

namespace BreganUtils
{
    public class TimezoneHelper
    {
        public static DateTime ConvertDateTimeToLocalTime(string timezoneId, DateTime dt)
        {
            var destinazionTZ = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);

            return TimeZoneInfo.ConvertTimeFromUtc(dt, destinazionTZ);
        }
    }
}