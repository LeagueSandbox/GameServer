using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class BasicTutorialMessageWindow : BasePacket
    {
        public BasicTutorialMessageWindow(string message)
            : base(PacketCmd.PKT_S2C_BASIC_TUTORIAL_MESSAGE_WINDOW)
        {
            // The following structure might be incomplete or wrong
            Write(message); // It seems to show up to 189 characters, which is strange
            Write(0x00);
        }
    }
}