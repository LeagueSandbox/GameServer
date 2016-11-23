using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleEmotion : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();
        private Logger _logger = Program.ResolveDependency<Logger>();

        public bool HandlePacket(Peer peer, byte[] data)
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

    public enum Emotions : byte
    {
        Dance = 0,
        Taunt = 1,
        Laugh = 2,
        Joke = 3
    }
}
