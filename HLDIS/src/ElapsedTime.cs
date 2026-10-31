using System;
using Vintagestory.API.Config;

namespace HLDIS
{
    /// <summary>
    /// Formats the elapsed time: days under 7 days, then weeks rounded to the nearest half week.
    /// </summary>
    public static class ElapsedTime
    {
        public const double DaysPerWeek = 7;

        public static bool IsLessThanADay(double days) => days < 1;

        public static string Format(double days)
        {
            if (days < DaysPerWeek)
            {
                int wholeDays = (int)Math.Floor(days);
                return wholeDays == 1
                    ? Lang.Get("hldis:elapsed-day")
                    : Lang.Get("hldis:elapsed-days", wholeDays);
            }

            // Round to the nearest half week: 7 d -> 1 week, 10 d -> a week and a half.
            int halfWeeks = Math.Max(2, (int)Math.Round(days / (DaysPerWeek / 2), MidpointRounding.AwayFromZero));
            int weeks = halfWeeks / 2;
            bool half = halfWeeks % 2 == 1;

            if (weeks == 1)
            {
                return Lang.Get(half ? "hldis:elapsed-week-half" : "hldis:elapsed-week");
            }
            return Lang.Get(half ? "hldis:elapsed-weeks-half" : "hldis:elapsed-weeks", weeks);
        }
    }
}
