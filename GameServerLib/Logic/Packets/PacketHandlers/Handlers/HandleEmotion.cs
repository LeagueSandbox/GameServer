using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleEmotion : PacketHandlerBase
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

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var emotion = new EmotionPacket(data);
            //for later use -> tracking, etc.
            var playerName = _playerManager.GetPeerInfo(peer).Champion.Model;
            switch (emotion.id)
            {
                case (byte)Emotions.Dance:
                    _logger.LogCoreInfo("Player " + playerName + " is dancing.");
                    break;
                case (byte)Emotions.Taunt:
                    _logger.LogCoreInfo("Player " + playerName + " is taunting.");
                    break;
                case (byte)Emotions.Laugh:
                    _logger.LogCoreInfo("Player " + playerName + " is laughing.");
                    break;
                case (byte)Emotions.Joke:
                    _logger.LogCoreInfo("Player " + playerName + " is joking.");
                    break;
            }

            var response = new EmotionPacket(emotion.id, emotion.netId);
            return _game.PacketHandlerManager.broadcastPacket(response, Channel.CHL_S2C);
        }
    }

    //TODO: move to separate file
    public enum Emotions : byte
    {
        Dance = 0,
        Taunt = 1,
        Laugh = 2,
        Joke = 3
    }
}
