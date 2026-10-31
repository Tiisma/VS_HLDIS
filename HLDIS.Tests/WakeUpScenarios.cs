using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.Api;
using Atlas.XUnit;
using Vintagestory.API.Common;
using Xunit;

namespace HLDIS.Tests
{
    /// <summary>
    /// Server-side behavior on a live headless server: what each joining player receives on the hldis channel.
    /// </summary>
    public class WakeUpScenarios : AtlasScenarioBase
    {
        [AtlasScenario]
        public async Task FirstJoin_Should_SendSeasonOnly()
        {
            ITestPlayer player = await World.JoinPlayer("Newcomer");
            WakeUpPacket packet = await Scenario.SingleWakeUp(World, player);

            IGameCalendar calendar = World.Api.World.Calendar;
            Assert.False(packet.HasPreviousVisit);
            Assert.Equal(0, packet.ElapsedDays);
            Assert.Equal(calendar.DaysPerMonth, packet.DaysPerMonth);
            Assert.InRange(packet.SeasonRel, calendar.GetSeasonRel(player.Position) - 0.01f, calendar.GetSeasonRel(player.Position) + 0.01f);
        }

        [AtlasScenario]
        public async Task Rejoin_Should_ReportInGameDaysSinceDisconnect()
        {
            ITestPlayer player = await World.JoinPlayer("Napper");
            await Scenario.SingleWakeUp(World, player);
            await Scenario.Kick(World, player);

            World.Api.World.Calendar.Add(10 * 24);

            ITestPlayer back = await World.JoinPlayer("Napper");
            WakeUpPacket packet = await Scenario.SingleWakeUp(World, back);

            Assert.True(packet.HasPreviousVisit);
            // Calendar.Add takes float hours, so allow for rounding.
            Assert.InRange(packet.ElapsedDays, 10 - 0.001, 10.5);
        }

        [AtlasScenario]
        public async Task Rejoin_Should_ReportLessThanADay_When_NoTimeWasAdded()
        {
            ITestPlayer player = await World.JoinPlayer("Blinker");
            await Scenario.SingleWakeUp(World, player);
            await Scenario.Kick(World, player);

            ITestPlayer back = await World.JoinPlayer("Blinker");
            WakeUpPacket packet = await Scenario.SingleWakeUp(World, back);

            Assert.True(packet.HasPreviousVisit);
            Assert.InRange(packet.ElapsedDays, 0, 1);
        }
    }

    internal static class Scenario
    {
        public static async Task<WakeUpPacket> SingleWakeUp(IWorldSession world, ITestPlayer player)
        {
            await world.Until(() => player.Client.Packets<WakeUpPacket>(HldisModSystem.ChannelName).Count > 0);
            IReadOnlyList<WakeUpPacket> packets = player.Client.Packets<WakeUpPacket>(HldisModSystem.ChannelName);
            return Assert.Single(packets);
        }

        public static async Task Kick(IWorldSession world, ITestPlayer player)
        {
            player.Player.Disconnect();
            await world.Until(() => !player.IsConnected);
        }
    }
}
