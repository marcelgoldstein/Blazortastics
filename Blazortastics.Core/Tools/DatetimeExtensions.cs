using System;

namespace Blazortastics.Core.Tools
{
    public static class DatetimeExtensions
    {
        public static DateTime GetFirstDayOfMonth(this DateTime date)
        {
            return date.AddDays(-(date.Day - 1));
        }

        public static DateTime GetLastDayOfMonth(this DateTime date)
        {
            return date.AddMonths(1).GetFirstDayOfMonth().AddDays(-1);
        }

        public static DateTime TruncateTime(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond);
        }

        public static DateTime MaximizeTime(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }
    }
}
