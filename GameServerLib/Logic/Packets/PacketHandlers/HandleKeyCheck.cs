using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Logging;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleKeyCheck : PacketHandlerBase
    {
        private readonly IPacketReader _packetReader;
        private readonly IPacketNotifier _packetNotifier;
        private readonly ILogger _logger;
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_KEY_CHECK;
        public override Channel PacketChannel => Channel.CHL_HANDSHAKE;

        public HandleKeyCheck(Game game)
        {
            _packetReader = game.PacketReader;
            _packetNotifier = game.PacketNotifier;
            _logger = LoggerProvider.GetLogger();
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _packetReader.ReadKeyCheckRequest(data);
            var userId = _game.Blowfish.Decrypt(request.CheckId);

            if (userId != request.UserId)
            {
                _logger.Warning("Client has sent wrong blowfish data.");
                return false;
            }

            if (request.VersionNo != Config.VERSION_NUMBER)
            {
                _logger.Warning("Client version doesn't match server's. " +
                                       $"(C:{request.VersionNo}, S:{Config.VERSION_NUMBER})");
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

                    _packetNotifier.NotifyKeyCheck(request.UserId, playerNo);

                    foreach (var p2 in _playerManager.GetPlayers())
                    {
                        if (p2.Item2.Peer != null && p2.Item2.UserId != player.UserId)
                        {
                            _packetNotifier.NotifyKeyCheck(player.Peer, p2.Item2.UserId, p2.Item2.PlayerNo);
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
