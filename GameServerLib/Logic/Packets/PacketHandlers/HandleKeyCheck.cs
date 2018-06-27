using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleKeyCheck : PacketHandlerBase
    {
        private readonly Logger _logger;
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_KEY_CHECK;
        public override Channel PacketChannel => Channel.CHL_HANDSHAKE;

        public HandleKeyCheck(Logger logger, Game game, PlayerManager playerManager)
        {
            _logger = logger;
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var keyCheck = new KeyCheckRequest(data);
            var userId = _game.Blowfish.Decrypt(keyCheck.CheckId);

            if (userId != keyCheck.UserId)
                return false;

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
                            _logger.LogCoreWarning("Ignoring new player " + userId + ", already connected!");
                            return false;
                        }
                    }

                    //TODO: add at least port or smth
                    p.Item1 = peer.Address.port;
                    player.Peer = peer;
                    player.PlayerNo = playerNo;
                    var response = new KeyCheckResponse(keyCheck.UserId, playerNo);
                    _game.PacketHandlerManager.BroadcastPacket(response, Channel.CHL_HANDSHAKE);


                    foreach(var p2 in _playerManager.GetPlayers())
                    {
                        if (p2.Item2.Peer != null && p2.Item2.UserId != player.UserId)
                        {
                            var response2 = new KeyCheckResponse(p2.Item2.UserId, p2.Item2.PlayerNo);
                            _game.PacketHandlerManager.SendPacket(player.Peer, response2, Channel.CHL_HANDSHAKE);
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
