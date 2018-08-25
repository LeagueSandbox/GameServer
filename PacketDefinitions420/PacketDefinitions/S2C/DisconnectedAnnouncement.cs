using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class DisconnectedAnnouncement : BasePacket
    {
        public DisconnectedAnnouncement(IAttackableUnit unit)
            : base(PacketCmd.PKT_S2C_DISCONNECTED_ANNOUNCEMENT)
        {
            WriteNetId(unit);
        }
    }
}