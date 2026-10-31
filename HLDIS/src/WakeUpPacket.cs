using System.Collections.Generic;
using ProtoBuf;

namespace HLDIS
{
    /// <summary>
    /// Sent by the server to the client when the player enters the game.
    /// </summary>
    [ProtoContract]
    public class WakeUpPacket
    {
        /// <summary>False on the player's first visit to this world.</summary>
        [ProtoMember(1)]
        public bool HasPreviousVisit;

        /// <summary>In-game days elapsed since the last disconnect.</summary>
        [ProtoMember(2)]
        public double ElapsedDays;

        /// <summary>IGameCalendar.GetSeasonRel at the player's position (0..1, hemisphere included).</summary>
        [ProtoMember(3)]
        public float SeasonRel;

        /// <summary>IGameCalendar.DaysPerMonth of the world, used to express long absences in months and years.</summary>
        [ProtoMember(4)]
        public int DaysPerMonth;
    }

    /// <summary>
    /// Data persisted in the world save.
    /// </summary>
    [ProtoContract]
    public class LastSeenData
    {
        [ProtoMember(1)]
        public int Version = 1;

        /// <summary>IGameCalendar.TotalDays when the player was last seen, keyed by PlayerUID.</summary>
        [ProtoMember(2)]
        public Dictionary<string, double> TotalDaysByPlayer = new Dictionary<string, double>();
    }
}
