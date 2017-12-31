using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleEmotion : PacketHandlerBase<EmotionPacketRequest>
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;
        private readonly Logger _logger;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_Emotion;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleEmotion(Game game, PlayerManager playerManager, Logger logger)
        {
            _game = game;
            _playerManager = playerManager;
            _logger = logger;
        }

        public override bool HandlePacketInternal(Peer peer, EmotionPacketRequest data)
        {
            //for later use -> tracking, etc.
            var playerName = _playerManager.GetPeerInfo(peer).Champion.Model;
            switch (data.EmoteId)
            {
                case Emotions.Dance:
                    _logger.LogCoreInfo("Player " + playerName + " is dancing.");
                    break;
                case Emotions.Taunt:
                    _logger.LogCoreInfo("Player " + playerName + " is taunting.");
                    break;
                case Emotions.Laugh:
                    _logger.LogCoreInfo("Player " + playerName + " is laughing.");
                    break;
                case Emotions.Joke:
                    _logger.LogCoreInfo("Player " + playerName + " is joking.");
                    break;
            }

            var response = new EmotionPacketResponse(data.EmoteId, data.NetId);
            return _game.PacketHandlerManager.broadcastPacket(response, Channel.CHL_S2C);
        }
    }
}
