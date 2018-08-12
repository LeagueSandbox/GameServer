using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class HideUi : BasePacket
    {
        public HideUi()
            : base(PacketCmd.PKT_S2C_HideUi)
        {

        }
    }
}