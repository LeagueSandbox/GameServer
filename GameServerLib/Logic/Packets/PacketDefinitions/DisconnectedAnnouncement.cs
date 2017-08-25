using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class DisconnectedAnnouncement : BasePacket
    {
        public DisconnectedAnnouncement(Unit unit) : base(PacketCmd.PKT_S2C_DisconnectedAnnouncement)
        {
            buffer.Write(unit.NetId);
        }
    }
}