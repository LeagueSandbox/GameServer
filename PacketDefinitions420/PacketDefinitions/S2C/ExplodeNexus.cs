using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class ExplodeNexus : BasePacket
    {
        public ExplodeNexus(INexus nexus)
            : base(PacketCmd.PKT_S2C_EXPLODE_NEXUS, nexus.NetId)
        {
            // animation ID?
            Write((byte)0xE7);
            Write((byte)0xF9);
            Write((byte)0x00);
            Write((byte)0x40);
            // unk
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);
        }
    }
}