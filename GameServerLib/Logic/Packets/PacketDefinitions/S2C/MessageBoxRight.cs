using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class MessageBoxRight : BasePacket
    {
        public MessageBoxRight(string message)
            : base(PacketCmd.PKT_S2C_MessageBoxRight)
        {
            // The following structure might be incomplete or wrong
            buffer.Write(Encoding.Default.GetBytes(message));
            buffer.Write(0x00);
        }
    }
}