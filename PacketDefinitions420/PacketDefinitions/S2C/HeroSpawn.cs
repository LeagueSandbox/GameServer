using GameServerCore.NetInfo;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class HeroSpawn : BasePacket
    {
        public HeroSpawn(ClientInfo player, int playerId)
            : base(PacketCmd.PKT_S2C_HERO_SPAWN)
        {
            WriteNetId(player.Champion);
            Write(playerId); // player Id
            Write((byte)40); // netNodeID ?
            Write((byte)0); // botSkillLevel Beginner=0 Intermediate=1
            if (player.Team == TeamId.TEAM_BLUE)
            {
                Write((byte)1); // teamNumber BotTeam=2,3 Blue=Order=1 Purple=Chaos=0
            }
            else
            {
                Write((byte)0); // teamNumber BotTeam=2,3 Blue=Order=1 Purple=Chaos=0
            }
            Write((byte)0); // isBot
            //buffer.Write((short)0; // botRank (deprecated as of 4.18)
            Write((byte)0); // spawnPosIndex
            Write(player.SkinNo);
			WriteConstLengthString(player.Name, 128);
			WriteConstLengthString(player.Champion.Model, 40);
            Write(0.0f); // deathDurationRemaining
            Write(0.0f); // timeSinceDeath
            Write(0); // UNK (4.18)
            Write((byte)0); // bitField
        }
    }
}