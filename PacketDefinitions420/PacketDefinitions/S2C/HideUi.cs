using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class HideUi : BasePacket
    {
        public HideUi()
            : base(PacketCmd.PKT_S2C_HIDE_UI)
        {

        }
    }
}