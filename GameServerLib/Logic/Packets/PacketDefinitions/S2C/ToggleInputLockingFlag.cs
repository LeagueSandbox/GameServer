using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ToggleInputLockingFlag : BasePacket
    {
        public ToggleInputLockingFlag(byte bitField, bool locked)
            : base(PacketCmd.PKT_S2_C_TOGGLE_INPUT_LOCKING_FLAG)
        {
            byte toggle = 0xFE;
            if (locked)
                toggle = 0xFF;
            _buffer.Write((byte)bitField); // 0x01 = centerCamera; 0x02 = movement; 0x04 = spells; etc
            _buffer.Write((byte)00);
            _buffer.Write((byte)00);
            _buffer.Write((byte)00);
            _buffer.Write((byte)toggle); // FE(nabled); FD(isabled);
        }
    }
}