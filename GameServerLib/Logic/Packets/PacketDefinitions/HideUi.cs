using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class HideUi : BasePacket
    {
        public HideUi() : base(PacketCmd.PKT_S2C_HideUi)
        {

        }
    }
}