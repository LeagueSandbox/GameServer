using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class ChampionDie2 : BasePacket
    {
        public ChampionDie2(IChampion die, float deathTimer) :
            base(PacketCmd.PKT_S2C_CHAMPION_DIE, die.NetId)
        {
            // Not sure what the whole purpose of that packet is
            Write((float)deathTimer);
        }
    }
}