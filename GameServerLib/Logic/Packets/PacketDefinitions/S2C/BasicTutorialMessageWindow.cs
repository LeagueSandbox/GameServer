using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class BasicTutorialMessageWindow : BasePacket
    {
        public BasicTutorialMessageWindow(string message) 
            : base(PacketCmd.PKT_S2_C_BASIC_TUTORIAL_MESSAGE_WINDOW)
        {
            // The following structure might be incomplete or wrong
            _buffer.Write(Encoding.Default.GetBytes(message)); // It seems to show up to 189 characters, which is strange
            _buffer.Write(0x00);
        }
    }
}