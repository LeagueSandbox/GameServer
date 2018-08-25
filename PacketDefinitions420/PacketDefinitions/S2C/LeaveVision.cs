using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class LeaveVision : BasePacket
    {
        public LeaveVision(IGameObject o)
            : base(PacketCmd.PKT_S2C_LEAVE_VISION, o.NetId)
        {
        }
    }
}