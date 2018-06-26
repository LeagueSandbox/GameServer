using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class LeaveVision : BasePacket
    {
        public LeaveVision(GameObject o)
            : base(PacketCmd.PKT_S2_C_LEAVE_VISION, o.NetId)
        {
        }
    }
}