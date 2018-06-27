using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ExplodeNexus : BasePacket
    {
        public ExplodeNexus(Nexus nexus)
            : base(PacketCmd.PKT_S2_C_EXPLODE_NEXUS, nexus.NetId)
        {
            // animation ID?
            _buffer.Write((byte)0xE7);
            _buffer.Write((byte)0xF9);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x40);
            // unk
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
        }
    }
}