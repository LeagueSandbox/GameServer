using System.Text;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class HeroSpawn : BasePacket
    {
        public HeroSpawn(ClientInfo player, int playerId)
            : base(PacketCmd.PKT_S2C_HeroSpawn)
        {
            buffer.Write((int)player.Champion.NetId);
            buffer.Write((int)playerId); // player Id
            buffer.Write((byte)40); // netNodeID ?
            buffer.Write((byte)0); // botSkillLevel Beginner=0 Intermediate=1
            if (player.Team == TeamId.TEAM_BLUE)
            {
                buffer.Write((byte)1); // teamNumber BotTeam=2,3 Blue=Order=1 Purple=Chaos=0
            }
            else
            {
                buffer.Write((byte)0); // teamNumber BotTeam=2,3 Blue=Order=1 Purple=Chaos=0
            }
            buffer.Write((byte)0); // isBot
            //buffer.Write((short)0; // botRank (deprecated as of 4.18)
            buffer.Write((byte)0); // spawnPosIndex
            buffer.Write((int)player.SkinNo);
            foreach (var b in Encoding.Default.GetBytes(player.Name))
                buffer.Write((byte)b);
            buffer.fill(0, 128 - player.Name.Length);
            foreach (var b in Encoding.Default.GetBytes(player.Champion.Model))
                buffer.Write((byte)b);
            buffer.fill(0, 40 - player.Champion.Model.Length);
            buffer.Write((float)0.0f); // deathDurationRemaining
            buffer.Write((float)0.0f); // timeSinceDeath
            buffer.Write((int)0); // UNK (4.18)
            buffer.Write((byte)0); // bitField
        }
    }
}