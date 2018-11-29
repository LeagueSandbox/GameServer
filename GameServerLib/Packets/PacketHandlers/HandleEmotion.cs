using GameServerCore;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleEmotion : PacketHandlerBase<EmotionPacketRequest>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;
        private readonly ILog _logger;

        public HandleEmotion(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
            _logger = LoggerProvider.GetLogger();
        }

        public override bool HandlePacket(int userId, EmotionPacketRequest req)
        {
            var champion = _playerManager.GetPeerInfo(userId).Champion;
            champion.StopChampionMovement();
            //for later use -> tracking, etc.
            var playerName = _playerManager.GetPeerInfo(userId).Champion.Model;
            switch (req.Id)
            {
                case Emotions.DANCE:
                    _logger.Debug("Player " + playerName + " is dancing.");
                    break;
                case Emotions.TAUNT:
                    _logger.Debug("Player " + playerName + " is taunting.");
                    break;
                case Emotions.LAUGH:
                    _logger.Debug("Player " + playerName + " is laughing.");
                    break;
                case Emotions.JOKE:
                    _logger.Debug("Player " + playerName + " is joking.");
                    break;
            }

            _game.PacketNotifier.NotifyEmotions(req.Id, req.NetId);
            return true;
        }
    }
}
