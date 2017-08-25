using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class ToggleInputLockingFlag : BasePacket
    {

        public ToggleInputLockingFlag(byte bitField, bool locked) : base(PacketCmd.PKT_S2C_ToggleInputLockingFlag)
        {
            byte toggle = 0xFE;
            if (locked)
                toggle = 0xFF;
            buffer.Write((byte)bitField); // 0x01 = centerCamera; 0x02 = movement; 0x04 = spells; etc
            buffer.Write((byte)00);
            buffer.Write((byte)00);
            buffer.Write((byte)00);
            buffer.Write((byte)toggle); // FE(nabled); FD(isabled);
        }
    }
}