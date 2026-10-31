using System;
using Vintagestory.API.Config;

namespace HLDIS
{
    /// <summary>
    /// Mise en forme de la durée écoulée : jours sous 7 jours, puis semaines arrondies à la demi-semaine.
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

            // Arrondi à la demi-semaine la plus proche : 7 j -> 1 sem., 10 j -> 1 sem. et demie.
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
