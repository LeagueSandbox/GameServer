using GameServerCore;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleMap : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CLIENT_READY;
        public override Channel PacketChannel => Channel.CHL_LOADING_SCREEN;

        public HandleMap(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, byte[] data)
        {
            // Builds team info e.g. first UserId set on Blue has PlayerId 0
            // increment by 1 for each added player
             _game.PacketNotifier.NotifyLoadScreenInfo(userId, _playerManager.GetPlayers());

            // Distributes each players info by UserId
            foreach (var player in _playerManager.GetPlayers())
            {
                // Giving the UserId in loading screen a name
                 _game.PacketNotifier.NotifyLoadScreenPlayerName(userId, player);
                // Giving the UserId in loading screen a champion
                 _game.PacketNotifier.NotifyLoadScreenPlayerChampion(userId, player);
            }

            return true;
        }
    }
}
