using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class LeaveVision : BasePacket
    {
        public LeaveVision(Game game, GameObject o)
            : base(game, PacketCmd.PKT_S2C_LEAVE_VISION, o.NetId)
        {
        }
    }
}