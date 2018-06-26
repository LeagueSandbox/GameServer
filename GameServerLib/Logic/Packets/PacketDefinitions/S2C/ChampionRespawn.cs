using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChampionRespawn : BasePacket
    {
        public ChampionRespawn(Champion c) : base(PacketCmd.PKT_S2_C_CHAMPION_RESPAWN, c.NetId)
        {
            _buffer.Write(c.X);
            _buffer.Write(c.Y);
            _buffer.Write(c.GetZ());
        }
    }
}