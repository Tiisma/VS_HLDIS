using System;
using Vintagestory.API.Config;

namespace HLDIS
{
    public enum EnumElapsedUnit
    {
        Days,
        Weeks,
        Months,
        Years
    }

    /// <summary>
    /// Elapsed time broken down into the unit shown to the player.
    /// </summary>
    public readonly struct ElapsedAmount
    {
        public readonly EnumElapsedUnit Unit;
        /// <summary>Whole days, weeks, months or years.</summary>
        public readonly int Count;
        /// <summary>Adds "and a half" (weeks and months only).</summary>
        public readonly bool Half;
        /// <summary>Remaining whole months (years only).</summary>
        public readonly int ExtraMonths;

        public ElapsedAmount(EnumElapsedUnit unit, int count, bool half = false, int extraMonths = 0)
        {
            Unit = unit;
            Count = count;
            Half = half;
            ExtraMonths = extraMonths;
        }
    }

    /// <summary>
    /// Formats the elapsed time: days under 7 days, weeks under a month, months under a year, then years and months.
    /// Weeks and months are rounded to the nearest half; months and years follow the world's calendar.
    /// </summary>
    public static class ElapsedTime
    {
        public const double DaysPerWeek = 7;
        // GameCalendar.DaysPerYear is 12 * DaysPerMonth in VintagestoryLib 1.22.7.
        public const int MonthsPerYear = 12;

        public static bool IsLessThanADay(double days) => days < 1;

        public static ElapsedAmount Measure(double days, int daysPerMonth)
        {
            daysPerMonth = Math.Max(1, daysPerMonth);

            if (days < DaysPerWeek)
            {
                return new ElapsedAmount(EnumElapsedUnit.Days, (int)Math.Floor(days));
            }

            if (days < daysPerMonth)
            {
                // 7 d -> 1 week, 10 d -> a week and a half.
                int halfWeeks = Math.Max(2, RoundToInt(days / (DaysPerWeek / 2)));
                return new ElapsedAmount(EnumElapsedUnit.Weeks, halfWeeks / 2, halfWeeks % 2 == 1);
            }

            int halfMonths = Math.Max(2, RoundToInt(days / (daysPerMonth / 2.0)));
            if (halfMonths < MonthsPerYear * 2)
            {
                return new ElapsedAmount(EnumElapsedUnit.Months, halfMonths / 2, halfMonths % 2 == 1);
            }

            int totalMonths = Math.Max(MonthsPerYear, RoundToInt(days / daysPerMonth));
            return new ElapsedAmount(EnumElapsedUnit.Years, totalMonths / MonthsPerYear, extraMonths: totalMonths % MonthsPerYear);
        }

        public static string Format(double days, int daysPerMonth)
        {
            ElapsedAmount amount = Measure(days, daysPerMonth);

            switch (amount.Unit)
            {
                case EnumElapsedUnit.Days:
                    return Counted("days", amount.Count, false);
                case EnumElapsedUnit.Weeks:
                    return Counted("weeks", amount.Count, amount.Half);
                case EnumElapsedUnit.Months:
                    return Counted("months", amount.Count, amount.Half);
                default:
                    string years = Counted("years", amount.Count, false);
                    if (amount.ExtraMonths == 0) return years;
                    return Lang.Get("hldis:elapsed-and", years, Counted("months", amount.ExtraMonths, false));
            }
        }

        // Keys: hldis:elapsed-day, -days, -week, -week-half, -weeks, -weeks-half, -month(s)..., -year, -years.
        static string Counted(string pluralUnit, int count, bool half)
        {
            string unit = count == 1 ? pluralUnit.Substring(0, pluralUnit.Length - 1) : pluralUnit;
            return Lang.Get("hldis:elapsed-" + unit + (half ? "-half" : ""), count);
        }

        static int RoundToInt(double value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);
    }
}
