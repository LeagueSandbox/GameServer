using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Enet;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct HeroSpawnArgs
    {
        public uint PlayerNetId { get; }
        public int PlayerId { get; }
        public TeamId PlayerTeam { get; }
        public int SkinNo { get; }
        public string PlayerName { get; }
        public string ChampionName { get; }

        public HeroSpawnArgs(uint playerNetId, int playerId, TeamId playerTeam, int skinNo, string playerName,
            string championName)
        {
            PlayerNetId = playerNetId;
            PlayerId = playerId;
            PlayerTeam = playerTeam;
            SkinNo = skinNo;
            PlayerName = playerName;
            ChampionName = championName;
        }
    }
}
