using System.Collections.Generic;
using GameServerCore;
using GameServerCore.NetInfo;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SynchVersionResponse : BasePacket
    {
        public SynchVersionResponse(List<Pair<uint, ClientInfo>> players, string version, string gameMode, int map)
            : base(PacketCmd.PKT_S2C_SYNCH_VERSION)
        {
            Write((byte)1); // Bit field
            // First bit: doVersionsMatch - If set to 0, the client closes
            // Second bit: Seems to enable a 'ClientMetricsLogger'
            Write((uint)map); // mapId
            foreach (var player in players)
            {
                var p = player.Item2;
                var summonerSpells = p.SummonerSkills;
                Write(p.UserId);
                Write((short)0x1E); // unk
                WriteStringHash(summonerSpells[0]);
                WriteStringHash(summonerSpells[1]);
                Write((byte)0); // bot boolean
                Write((int)p.Team); // Probably a short
                Fill(0, 64); // name is no longer here
                Fill(0, 64);
				WriteConstLengthString(p.Rank, 24);
                Write(p.Icon);
                Write(p.Ribbon);
            }

            for (var i = 0; i < 12 - players.Count; ++i)
            {
                Write((long)-1);
                Fill(0, 173);
            }
			WriteConstLengthString(version, 256);
			WriteConstLengthString(gameMode, 128);

			Write("NA1");
            Fill(0, 2333); // 128 - 3 + 661 + 1546
            Write((uint)487826); // gameFeatures (turret range indicators, etc.)
            Fill(0, 256);
            Write((uint)0);
            Fill(1, 19);
        }
    }
}