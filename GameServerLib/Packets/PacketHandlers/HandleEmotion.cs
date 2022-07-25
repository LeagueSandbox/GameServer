using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Logging;
using log4net;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleEmotion : PacketHandlerBase<EmotionPacketRequest>
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;
        private static ILog _logger = LoggerProvider.GetLogger();

        public HandleEmotion(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, EmotionPacketRequest req)
        {
            var champion = _playerManager.GetPeerInfo(userId).Champion;
            champion.StopMovement();
            champion.UpdateMoveOrder(GameServerCore.Enums.OrderType.Taunt);
            //for later use -> tracking, etc.
            var playerName = _playerManager.GetPeerInfo(userId).Champion.Model;
            switch (req.EmoteID)
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

            _game.PacketNotifier.NotifyS2C_PlayEmote((Emotions)req.EmoteID, champion.NetId);
            return true;
        }
    }
}
