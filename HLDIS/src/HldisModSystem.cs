using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace HLDIS
{
    /// <summary>
    /// Serveur : mémorise la date de jeu de la dernière présence de chaque joueur et envoie le temps écoulé à son entrée en jeu.
    /// Client : traduit et affiche le message dans le chat, dans la langue du joueur.
    /// </summary>
    public class HldisModSystem : ModSystem
    {
        const string ChannelName = "hldis";
        const string SaveKey = "hldis-lastseen";

        ICoreServerAPI sapi;
        IServerNetworkChannel serverChannel;
        LastSeenData lastSeen = new LastSeenData();
        // Joueurs ayant reçu leur message pendant la session ; seuls eux voient leur date mise à jour.
        readonly HashSet<string> activePlayers = new HashSet<string>();

        ICoreClientAPI capi;

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;
            serverChannel = api.Network.RegisterChannel(ChannelName).RegisterMessageType<WakeUpPacket>();

            api.Event.SaveGameLoaded += OnSaveGameLoaded;
            api.Event.GameWorldSave += OnGameWorldSave;
            api.Event.PlayerNowPlaying += OnPlayerNowPlaying;
            api.Event.PlayerDisconnect += OnPlayerDisconnect;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            capi = api;
            api.Network.RegisterChannel(ChannelName)
                .RegisterMessageType<WakeUpPacket>()
                .SetMessageHandler<WakeUpPacket>(OnWakeUp);
        }

        void OnSaveGameLoaded()
        {
            lastSeen = sapi.WorldManager.SaveGame.GetData<LastSeenData>(SaveKey) ?? new LastSeenData();
            lastSeen.TotalDaysByPlayer ??= new Dictionary<string, double>();
        }

        void OnGameWorldSave()
        {
            double now = sapi.World.Calendar.TotalDays;
            foreach (string uid in activePlayers)
            {
                lastSeen.TotalDaysByPlayer[uid] = now;
            }
            sapi.WorldManager.SaveGame.StoreData(SaveKey, lastSeen);
        }

        void OnPlayerNowPlaying(IServerPlayer player)
        {
            IGameCalendar calendar = sapi.World.Calendar;
            double now = calendar.TotalDays;
            bool hasPrevious = lastSeen.TotalDaysByPlayer.TryGetValue(player.PlayerUID, out double previous);

            BlockPos pos = player.Entity?.Pos.AsBlockPos ?? sapi.World.DefaultSpawnPosition.AsBlockPos;

            serverChannel.SendPacket(new WakeUpPacket
            {
                HasPreviousVisit = hasPrevious,
                ElapsedDays = hasPrevious ? System.Math.Max(0, now - previous) : 0,
                SeasonRel = calendar.GetSeasonRel(pos)
            }, player);

            activePlayers.Add(player.PlayerUID);
            lastSeen.TotalDaysByPlayer[player.PlayerUID] = now;
        }

        void OnPlayerDisconnect(IServerPlayer player)
        {
            // Un joueur déconnecté avant d'être entré en jeu garde sa date précédente.
            if (!activePlayers.Remove(player.PlayerUID)) return;
            lastSeen.TotalDaysByPlayer[player.PlayerUID] = sapi.World.Calendar.TotalDays;
        }

        void OnWakeUp(WakeUpPacket packet)
        {
            string season = Lang.Get(SeasonPhase.LangKey(packet.SeasonRel));
            string message;

            if (!packet.HasPreviousVisit)
            {
                message = Lang.Get("hldis:wakeup-first", season);
            }
            else if (ElapsedTime.IsLessThanADay(packet.ElapsedDays))
            {
                message = Lang.Get("hldis:wakeup-short", season);
            }
            else
            {
                message = Lang.Get("hldis:wakeup", ElapsedTime.Format(packet.ElapsedDays), season);
            }

            capi.ShowChatMessage(message);
        }
    }
}
