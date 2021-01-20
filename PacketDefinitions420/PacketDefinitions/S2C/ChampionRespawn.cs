using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class ChampionRespawn : BasePacket
    {
        public ChampionRespawn(IChampion c) :
            base(PacketCmd.PKT_S2C_CHAMPION_RESPAWN, c.NetId)
        {
            Write((float)c.Position.X);
            Write((float)c.Position.Y);
            Write((float)c.GetHeight());
        }
    }
}