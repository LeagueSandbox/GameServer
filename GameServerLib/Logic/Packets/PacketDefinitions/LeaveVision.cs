using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class LeaveVision : BasePacket
    {
        public LeaveVision(GameObject o) : base(PacketCmd.PKT_S2C_LeaveVision, o.NetId)
        {
        }
    }
}