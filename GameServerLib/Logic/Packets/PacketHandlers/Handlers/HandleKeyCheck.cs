using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleKeyCheck : PacketHandlerBase
    {
        private readonly Logger _logger;
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_KeyCheck;
        public override Channel PacketChannel => Channel.CHL_HANDSHAKE;

        public HandleKeyCheck(Logger logger, Game game, PlayerManager playerManager)
        {
            _logger = logger;
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var keyCheck = new KeyCheck(data);
            var userId = _game.Blowfish.Decrypt(keyCheck.checkId);

            if (userId != keyCheck.userId)
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
                    var response = new KeyCheck(keyCheck.userId, playerNo);
                    bool bRet = _game.PacketHandlerManager.sendPacket(peer, response, Channel.CHL_HANDSHAKE);
                    //HandleGameNumber(player, peer, _game);//Send 0x91 Packet?
                    return true;
                }
                ++playerNo;
            }
            return false;
        }

        bool HandleGameNumber(ClientInfo client, Peer peer, Game game)
        {
            var world = new WorldSendGameNumber(1, client.Name);
            return _game.PacketHandlerManager.sendPacket(peer, world, Channel.CHL_S2C);
        }
    }
}
