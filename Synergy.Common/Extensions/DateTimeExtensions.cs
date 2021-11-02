using System;

namespace Synergy.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime source, DayOfWeek startOfWeek)
        {
            var diff = source.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return source.AddDays(-1 * diff).Date;
        }

        public static DateTime EndOfWeek(this DateTime source, DayOfWeek startOfWeek)
        {
            return source.StartOfWeek(startOfWeek).AddDays(7);
        }
    }
}