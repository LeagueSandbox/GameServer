using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandlePauseReq : PacketHandlerBase
    {

        public override PacketCmd PacketType => PacketCmd.PKT_PAUSEGame;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            Game.Pause();
            return true;
        }
    }
}