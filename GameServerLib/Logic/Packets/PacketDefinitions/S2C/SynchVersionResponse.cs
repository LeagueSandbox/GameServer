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
            : base(PacketCmd.PKT_S2_C_SYNCH_VERSION)
        {
            _buffer.Write((byte)1); // Bit field
            // First bit: doVersionsMatch - If set to 0, the client closes
            // Second bit: Seems to enable a 'ClientMetricsLogger'
            _buffer.Write((uint)map); // mapId
            foreach (var player in players)
            {
                var p = player.Item2;
                var summonerSpells = p.SummonerSkills;
                _buffer.Write((long)p.UserId);
                _buffer.Write((short)0x1E); // unk
                _buffer.Write((uint)HashFunctions.HashString(summonerSpells[0]));
                _buffer.Write((uint)HashFunctions.HashString(summonerSpells[1]));
                _buffer.Write((byte)0); // bot boolean
                _buffer.Write((int)p.Team); // Probably a short
                _buffer.Fill(0, 64); // name is no longer here
                _buffer.Fill(0, 64);
                foreach (var b in Encoding.Default.GetBytes(p.Rank))
                    _buffer.Write((byte)b);
                _buffer.Fill(0, 24 - p.Rank.Length);
                _buffer.Write((int)p.Icon);
                _buffer.Write((short)p.Ribbon);
            }

            for (var i = 0; i < 12 - players.Count; ++i)
            {
                _buffer.Write((long)-1);
                _buffer.Fill(0, 173);
            }
            foreach (var b in Encoding.Default.GetBytes(version))
                _buffer.Write((byte)b);
            _buffer.Fill(0, 256 - version.Length);
            foreach (var b in Encoding.Default.GetBytes(gameMode))
                _buffer.Write((byte)b);
            _buffer.Fill(0, 128 - gameMode.Length);

            foreach (var b in Encoding.Default.GetBytes("NA1"))
                _buffer.Write((byte)b);
            _buffer.Fill(0, 2333); // 128 - 3 + 661 + 1546
            _buffer.Write((uint)487826); // gameFeatures (turret range indicators, etc.)
            _buffer.Fill(0, 256);
            _buffer.Write((uint)0);
            _buffer.Fill(1, 19);
        }
    }
}