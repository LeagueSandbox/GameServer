using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class ToggleInputLockingFlag : BasePacket
    {
        public ToggleInputLockingFlag(byte bitField, bool locked)
            : base(PacketCmd.PKT_S2C_TOGGLE_INPUT_LOCKING_FLAG)
        {
            byte toggle = 0xFE;
            if (locked)
                toggle = 0xFF;
            Write(bitField); // 0x01 = centerCamera; 0x02 = movement; 0x04 = spells; etc
            Write((byte)00);
            Write((byte)00);
            Write((byte)00);
            Write(toggle); // FE(nabled); FD(isabled);
        }
    }
}