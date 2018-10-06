using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSurrender : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly IPlayerManager _pm;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SURRENDER;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSurrender(Game game)
        {
            _game = game;
            _pm = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, byte[] data)
        {
            var c = _pm.GetPeerInfo(userId).Champion;
             _game.PacketNotifier.NotifySurrender(c, 0x03, 1, 0, 5, c.Team, 10.0f);
            return true;
        }
    }
}
