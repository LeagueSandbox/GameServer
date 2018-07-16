using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandlePauseReq : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_PAUSE_GAME;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public HandlePauseReq(Game game)
        {
            _game = game;
            _playerManager = game.GetPlayerManager();
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            _game.Pause();
            return true;
        }
    }
}