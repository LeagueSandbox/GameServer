using System.Text;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class HeroSpawn : BasePacket
    {
        public HeroSpawn(ClientInfo player, int playerId)
            : base(PacketCmd.PKT_S2_C_HERO_SPAWN)
        {
            _buffer.Write((int)player.Champion.NetId);
            _buffer.Write(playerId); // player Id
            _buffer.Write((byte)40); // netNodeID ?
            _buffer.Write((byte)0); // botSkillLevel Beginner=0 Intermediate=1
            if (player.Team == TeamId.TEAM_BLUE)
            {
                _buffer.Write((byte)1); // teamNumber BotTeam=2,3 Blue=Order=1 Purple=Chaos=0
            }
            else
            {
                _buffer.Write((byte)0); // teamNumber BotTeam=2,3 Blue=Order=1 Purple=Chaos=0
            }
            _buffer.Write((byte)0); // isBot
            //buffer.Write((short)0; // botRank (deprecated as of 4.18)
            _buffer.Write((byte)0); // spawnPosIndex
            _buffer.Write(player.SkinNo);
            foreach (var b in Encoding.Default.GetBytes(player.Name))
                _buffer.Write(b);
            _buffer.Fill(0, 128 - player.Name.Length);
            foreach (var b in Encoding.Default.GetBytes(player.Champion.Model))
                _buffer.Write(b);
            _buffer.Fill(0, 40 - player.Champion.Model.Length);
            _buffer.Write(0.0f); // deathDurationRemaining
            _buffer.Write(0.0f); // timeSinceDeath
            _buffer.Write(0); // UNK (4.18)
            _buffer.Write((byte)0); // bitField
        }
    }
}