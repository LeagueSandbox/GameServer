using ENet;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleMoveConfirm : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_MoveConfirm;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            return true;
        }
    }
}
