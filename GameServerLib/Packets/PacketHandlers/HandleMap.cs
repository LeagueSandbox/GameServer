using GameServerCore;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleMap : PacketHandlerBase<MapRequest>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public HandleMap(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, MapRequest req)
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
