using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class MessageBoxRight : BasePacket
    {
        public MessageBoxRight(string message)
            : base(PacketCmd.PKT_S2C_MESSAGE_BOX_RIGHT)
        {
            // The following structure might be incomplete or wrong
            Write(message);
            Write(0x00);
        }
    }
}