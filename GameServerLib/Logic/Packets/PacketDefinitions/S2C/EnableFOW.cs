using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class EnableFOW : BasePacket
    {
        public EnableFOW(bool activate)
            : base(PacketCmd.PKT_S2C_EnableFOW)
        {
            buffer.Write(activate ? 0x01 : 0x00);
        }
    }
}