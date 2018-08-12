using System.Collections.Generic;
using System.Text;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SynchVersionResponse : BasePacket
    {
        public SynchVersionResponse(List<Pair<uint, ClientInfo>> players, string version, string gameMode, int map)
            : base(PacketCmd.PKT_S2C_SynchVersion)
        {
            buffer.Write((byte)1); // Bit field
            // First bit: doVersionsMatch - If set to 0, the client closes
            // Second bit: Seems to enable a 'ClientMetricsLogger'
            buffer.Write((uint)map); // mapId
            foreach (var player in players)
            {
                var p = player.Item2;
                var summonerSpells = p.SummonerSkills;
                buffer.Write((long)p.UserId);
                buffer.Write((short)0x1E); // unk
                buffer.Write((uint)HashFunctions.HashString(summonerSpells[0]));
                buffer.Write((uint)HashFunctions.HashString(summonerSpells[1]));
                buffer.Write((byte)0); // bot boolean
                buffer.Write((int)p.Team); // Probably a short
                buffer.fill(0, 64); // name is no longer here
                buffer.fill(0, 64);
                foreach (var b in Encoding.Default.GetBytes(p.Rank))
                    buffer.Write((byte)b);
                buffer.fill(0, 24 - p.Rank.Length);
                buffer.Write((int)p.Icon);
                buffer.Write((short)p.Ribbon);
            }

            for (var i = 0; i < 12 - players.Count; ++i)
            {
                buffer.Write((long)-1);
                buffer.fill(0, 173);
            }
            foreach (var b in Encoding.Default.GetBytes(version))
                buffer.Write((byte)b);
            buffer.fill(0, 256 - version.Length);
            foreach (var b in Encoding.Default.GetBytes(gameMode))
                buffer.Write((byte)b);
            buffer.fill(0, 128 - gameMode.Length);

            foreach (var b in Encoding.Default.GetBytes("NA1"))
                buffer.Write((byte)b);
            buffer.fill(0, 2333); // 128 - 3 + 661 + 1546
            buffer.Write((uint)487826); // gameFeatures (turret range indicators, etc.)
            buffer.fill(0, 256);
            buffer.Write((uint)0);
            buffer.fill(1, 19);
        }
    }
}