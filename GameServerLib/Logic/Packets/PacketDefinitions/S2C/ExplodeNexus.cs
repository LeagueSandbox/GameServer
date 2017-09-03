using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ExplodeNexus : BasePacket
    {
        public ExplodeNexus(uint nexusId)
            : base(PacketCmd.PKT_S2C_ExplodeNexus, nexusId)
        {
            // animation ID?
            buffer.Write((byte)0xE7);
            buffer.Write((byte)0xF9);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x40);
            // unk
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
        }
    }
}