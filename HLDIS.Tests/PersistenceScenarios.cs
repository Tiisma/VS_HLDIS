using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atlas.Api;
using Atlas.XUnit;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace HLDIS.Tests
{
    /// <summary>
    /// The last-seen date must survive a genuine server restart (world save round trip).
    /// Scenarios run in name order: Step1 seeds, Step2 restarts and checks.
    /// </summary>
    [TestCaseOrderer("HLDIS.Tests.AlphabeticalOrderer", "HLDIS.Tests")]
    public class PersistenceScenarios : AtlasScenarioBase
    {
        const double AddedDays = 30;

        [AtlasScenario]
        public async Task Step1_Seed_LastSeenThenAdvanceTime()
        {
            ITestPlayer player = await World.JoinPlayer("Hibernator");
            await Scenario.SingleWakeUp(World, player);
            await Scenario.Kick(World, player);

            World.Api.World.Calendar.Add((float)(AddedDays * 24));
        }

        [AtlasScenario(RestartWorld = true)]
        public async Task Step2_Rejoin_AfterRestart_Should_RememberLastSeen()
        {
            ITestPlayer player = await World.JoinPlayer("Hibernator");
            WakeUpPacket packet = await Scenario.SingleWakeUp(World, player);

            Assert.True(packet.HasPreviousVisit);
            // Calendar.Add takes float hours, so allow for rounding.
            Assert.InRange(packet.ElapsedDays, AddedDays - 0.001, AddedDays + 1);
        }
    }

    public class AlphabeticalOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
            return testCases.OrderBy(t => t.TestMethod.Method.Name, StringComparer.Ordinal);
        }
    }
}
