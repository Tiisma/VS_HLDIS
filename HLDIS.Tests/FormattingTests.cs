using Vintagestory.API.Common;
using Xunit;

namespace HLDIS.Tests
{
    /// <summary>
    /// Pure logic, no server boot.
    /// </summary>
    public class FormattingTests
    {
        [Theory]
        [InlineData(0.5, 9, EnumElapsedUnit.Days, 0, false, 0)]
        [InlineData(6.99, 9, EnumElapsedUnit.Days, 6, false, 0)]
        [InlineData(7, 9, EnumElapsedUnit.Weeks, 1, false, 0)]
        [InlineData(8.75, 9, EnumElapsedUnit.Weeks, 1, true, 0)]
        [InlineData(10, 9, EnumElapsedUnit.Months, 1, false, 0)]
        [InlineData(12.3, 9, EnumElapsedUnit.Months, 1, true, 0)]
        [InlineData(105, 9, EnumElapsedUnit.Months, 11, true, 0)]
        [InlineData(107, 9, EnumElapsedUnit.Years, 1, false, 0)]
        [InlineData(140, 9, EnumElapsedUnit.Years, 1, false, 4)]
        [InlineData(20, 30, EnumElapsedUnit.Weeks, 3, false, 0)]
        [InlineData(400, 30, EnumElapsedUnit.Years, 1, false, 1)]
        [InlineData(7, 3, EnumElapsedUnit.Months, 2, true, 0)]
        public void Measure_Should_PickUnitAndRounding(double days, int daysPerMonth, EnumElapsedUnit unit, int count, bool half, int extraMonths)
        {
            ElapsedAmount amount = ElapsedTime.Measure(days, daysPerMonth);

            Assert.Equal(unit, amount.Unit);
            Assert.Equal(count, amount.Count);
            Assert.Equal(half, amount.Half);
            Assert.Equal(extraMonths, amount.ExtraMonths);
        }

        [Theory]
        [InlineData(0.00f, EnumSeason.Winter, EnumSeasonPart.Early)]
        [InlineData(0.10f, EnumSeason.Winter, EnumSeasonPart.Mid)]
        [InlineData(0.20f, EnumSeason.Winter, EnumSeasonPart.Late)]
        [InlineData(0.22f, EnumSeason.Spring, EnumSeasonPart.Early)]
        [InlineData(0.40f, EnumSeason.Spring, EnumSeasonPart.Late)]
        [InlineData(0.60f, EnumSeason.Summer, EnumSeasonPart.Mid)]
        [InlineData(0.73f, EnumSeason.Fall, EnumSeasonPart.Early)]
        [InlineData(0.97f, EnumSeason.Fall, EnumSeasonPart.Late)]
        [InlineData(0.98f, EnumSeason.Winter, EnumSeasonPart.Early)]
        public void SeasonPhase_Should_SplitSeasonsIntoThirds(float seasonRel, EnumSeason season, EnumSeasonPart part)
        {
            SeasonPhase.Resolve(seasonRel, out EnumSeason actualSeason, out EnumSeasonPart actualPart);

            Assert.Equal(season, actualSeason);
            Assert.Equal(part, actualPart);
        }
    }
}
