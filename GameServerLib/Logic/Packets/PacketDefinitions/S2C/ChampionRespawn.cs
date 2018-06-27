using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChampionRespawn : BasePacket
    {
        public ChampionRespawn(Champion c) : base(PacketCmd.PKT_S2C_CHAMPION_RESPAWN, c.NetId)
        {
            _buffer.Write((float)c.X);
            _buffer.Write((float)c.Y);
            _buffer.Write((float)c.GetZ());
        }
    }
}