using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class DisconnectedAnnouncement : BasePacket
    {
        public DisconnectedAnnouncement(AttackableUnit unit)
            : base(PacketCmd.PKT_S2C_DISCONNECTED_ANNOUNCEMENT)
        {
            WriteNetId(unit);
        }
    }
}