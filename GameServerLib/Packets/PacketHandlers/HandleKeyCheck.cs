using ENet;
using GameServerCore.Packets.Enums;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleKeyCheck : PacketHandlerBase
    {
        private readonly ILogger _logger;
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_KEY_CHECK;
        public override Channel PacketChannel => Channel.CHL_HANDSHAKE;

        public HandleKeyCheck(Game game)
        {
            _logger = LoggerProvider.GetLogger();
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _game.PacketReader.ReadKeyCheckRequest(data);
            var userId = _game.Blowfish.Decrypt(request.CheckId);

            if (userId != request.UserId)
            {
                _logger.Warning("Client has sent wrong blowfish data.");
                return false;
            }

            var playerNo = 0;

            foreach (var p in _playerManager.GetPlayers())
            {
                var player = p.Item2;
                if (player.UserId == userId)
                {
                    if (player.Peer != null)
                    {
                        if (!player.IsDisconnected)
                        {
                            _logger.Warning($"Ignoring new player {userId}, already connected!");
                            return false;
                        }
                    }

                    //TODO: add at least port or smth
                    p.Item1 = peer.Address.port;
                    player.Peer = peer;
                    player.PlayerNo = playerNo;

                    _game.PacketNotifier.NotifyKeyCheck(request.UserId, playerNo);

                    foreach (var p2 in _playerManager.GetPlayers())
                    {
                        if (p2.Item2.Peer != null && p2.Item2.UserId != player.UserId)
                        {
                             _game.PacketNotifier.NotifyKeyCheck(player.Peer, p2.Item2.UserId, p2.Item2.PlayerNo);
                        }
                    }

                    return true;
                }
                ++playerNo;
            }
            return false;
        }
    }
}
