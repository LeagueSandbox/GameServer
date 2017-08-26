using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChampionRespawn : BasePacket
    {
        public ChampionRespawn(Champion c) : base(PacketCmd.PKT_S2C_ChampionRespawn, c.NetId)
        {
            buffer.Write(c.X);
            buffer.Write(c.Y);
            buffer.Write(c.GetZ());
        }
    }
}