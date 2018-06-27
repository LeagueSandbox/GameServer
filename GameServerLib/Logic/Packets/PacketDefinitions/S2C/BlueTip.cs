using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class BlueTip : BasePacket
    {
        public BlueTip(string title,
            string text,
            string imagePath,
            byte tipCommand,
            uint playernetid,
            uint netid)
            : base(PacketCmd.PKT_S2C_BLUE_TIP, playernetid)
        {
            _buffer.Write(Encoding.Default.GetBytes(text));
            _buffer.Fill(0, 128 - text.Length);

            _buffer.Write(Encoding.Default.GetBytes(title));
            _buffer.Fill(0, 128 - title.Length);

            _buffer.Write(Encoding.Default.GetBytes(imagePath));
            _buffer.Fill(0, 128 - imagePath.Length);

            _buffer.Write((byte)tipCommand); /* ACTIVATE_TIP     = 0
                                               REMOVE_TIP       = 1
                                               ENABLE_TIP_EVENTS  = 2
                                               DISABLE_TIP_EVENTS  = 3
                                               ACTIVATE_TIP_DIALOGUE  = 4
                                               ENABLE_TIP_DIALOGUE_EVENTS  = 5
                                               DISABLE_TIP_DIALOGUE_EVENTS  = 6 */
            _buffer.Write((int)netid);
        }
    }
}