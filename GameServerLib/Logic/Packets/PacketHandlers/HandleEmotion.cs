using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Logging;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleEmotion : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;
        private readonly ILogger _logger;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_EMOTION;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleEmotion(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
            _logger = LoggerProvider.GetLogger();
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _game.PacketReader.ReadEmotionPacketRequest(data);
            //for later use -> tracking, etc.
            var playerName = _playerManager.GetPeerInfo(peer).Champion.Model;
            switch (request.Id)
            {
                case Emotions.DANCE:
                    _logger.Info("Player " + playerName + " is dancing.");
                    break;
                case Emotions.TAUNT:
                    _logger.Info("Player " + playerName + " is taunting.");
                    break;
                case Emotions.LAUGH:
                    _logger.Info("Player " + playerName + " is laughing.");
                    break;
                case Emotions.JOKE:
                    _logger.Info("Player " + playerName + " is joking.");
                    break;
            }

             _game.PacketNotifier.NotifyEmotions(request.Id, request.NetId);
            return true;
        }
    }
}
