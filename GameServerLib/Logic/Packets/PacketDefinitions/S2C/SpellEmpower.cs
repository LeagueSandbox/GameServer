using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpellEmpower : BasePacket
    {
        public SpellEmpower(Unit unit, byte slot, byte empowerLevel)
            : base(PacketCmd.PKT_S2C_SpellEmpower, unit.NetId)
        {
            buffer.Write((byte)slot);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x06); // Unknown
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)empowerLevel); // 0 - normal, 1 - empowered (for Rengar)
        }
    }
}