using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class EnableFow : BasePacket
    {
        public EnableFow(bool activate)
            : base(PacketCmd.PKT_S2_C_ENABLE_FOW)
        {
            _buffer.Write(activate ? 0x01 : 0x00);
        }
    }
}