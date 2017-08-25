using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class DeleteObjectFromVision : BasePacket
    {
        public DeleteObjectFromVision(GameObject o) : base(PacketCmd.PKT_S2C_DeleteObject, o.NetId)
        {
        }
    }
}