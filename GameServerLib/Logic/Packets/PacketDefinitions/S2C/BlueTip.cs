using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class BlueTip : BasePacket
    {
        public BlueTip(Game game, 
            string title,
            string text,
            string imagePath,
            byte tipCommand,
            uint playernetid,
            uint netid)
            : base(game, PacketCmd.PKT_S2C_BLUE_TIP, playernetid)
        {
            WriteConstLengthString(text, 128);

            WriteConstLengthString(title, 128);

            WriteConstLengthString(imagePath, 128);

            Write((byte)tipCommand); /* ACTIVATE_TIP     = 0
                                               REMOVE_TIP       = 1
                                               ENABLE_TIP_EVENTS  = 2
                                               DISABLE_TIP_EVENTS  = 3
                                               ACTIVATE_TIP_DIALOGUE  = 4
                                               ENABLE_TIP_DIALOGUE_EVENTS  = 5
                                               DISABLE_TIP_DIALOGUE_EVENTS  = 6 */
            Write((int)netid);
        }
    }
}