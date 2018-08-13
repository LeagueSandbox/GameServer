using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class EnableFow : BasePacket
    {
        public EnableFow(bool activate)
            : base(PacketCmd.PKT_S2C_ENABLE_FOW)
        {
            Write(activate ? 0x01 : 0x00);
        }
    }
}