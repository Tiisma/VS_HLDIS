using System.Collections.Generic;
using ProtoBuf;

namespace HLDIS
{
    /// <summary>
    /// Envoyé par le serveur au client quand le joueur entre en jeu.
    /// </summary>
    [ProtoContract]
    public class WakeUpPacket
    {
        /// <summary>Faux lors de la première connexion du joueur à ce monde.</summary>
        [ProtoMember(1)]
        public bool HasPreviousVisit;

        /// <summary>Jours de jeu écoulés depuis la dernière déconnexion.</summary>
        [ProtoMember(2)]
        public double ElapsedDays;

        /// <summary>Valeur de IGameCalendar.GetSeasonRel à la position du joueur (0..1, hémisphère inclus).</summary>
        [ProtoMember(3)]
        public float SeasonRel;
    }

    /// <summary>
    /// Données persistées dans la sauvegarde du monde.
    /// </summary>
    [ProtoContract]
    public class LastSeenData
    {
        [ProtoMember(1)]
        public int Version = 1;

        /// <summary>IGameCalendar.TotalDays à la dernière présence connue, par PlayerUID.</summary>
        [ProtoMember(2)]
        public Dictionary<string, double> TotalDaysByPlayer = new Dictionary<string, double>();
    }
}
