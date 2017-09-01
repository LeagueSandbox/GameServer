using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class MessageBoxTop : BasePacket
    {
        public MessageBoxTop(string message) : base(PacketCmd.PKT_S2C_MessageBoxTop)
        {
            // The following structure might be incomplete or wrong
            buffer.Write(Encoding.Default.GetBytes(message));
            buffer.Write(0x00);
        }
    }
}