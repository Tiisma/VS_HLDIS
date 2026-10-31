using System;
using Vintagestory.API.Common;

namespace HLDIS
{
    public enum EnumSeasonPart
    {
        Early,
        Mid,
        Late
    }

    /// <summary>
    /// Découpe chaque saison en début / milieu / fin à partir de GetSeasonRel.
    /// </summary>
    public static class SeasonPhase
    {
        // Seuils relevés dans l'IL de Vintagestory.Common.GameCalendar.GetSeason (VintagestoryLib 1.22.7) :
        // < 0.2164 hiver, < 0.4685 printemps, < 0.726 été, < 0.9726 automne, sinon hiver.
        const float SpringStart = 0.2164f;
        const float SummerStart = 0.4685f;
        const float FallStart = 0.726f;
        const float WinterStart = 0.9726f;

        public static void Resolve(float seasonRel, out EnumSeason season, out EnumSeasonPart part)
        {
            float rel = seasonRel - (float)Math.Floor(seasonRel);
            float start, end;

            if (rel < SpringStart) { season = EnumSeason.Winter; start = WinterStart - 1; end = SpringStart; }
            else if (rel < SummerStart) { season = EnumSeason.Spring; start = SpringStart; end = SummerStart; }
            else if (rel < FallStart) { season = EnumSeason.Summer; start = SummerStart; end = FallStart; }
            else if (rel < WinterStart) { season = EnumSeason.Fall; start = FallStart; end = WinterStart; }
            else { season = EnumSeason.Winter; start = WinterStart; end = SpringStart + 1; }

            float progress = (rel - start) / (end - start);
            part = (EnumSeasonPart)Math.Clamp((int)(progress * 3), 0, 2);
        }

        /// <summary>Clé de langue, par exemple "hldis:season-mid-winter".</summary>
        public static string LangKey(float seasonRel)
        {
            Resolve(seasonRel, out EnumSeason season, out EnumSeasonPart part);
            return "hldis:season-" + part.ToString().ToLowerInvariant() + "-" + season.ToString().ToLowerInvariant();
        }
    }
}
