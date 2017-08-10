using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleMap : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_ClientReady;
        public override Channel PacketChannel => Channel.CHL_LOADING_SCREEN;

        public HandleMap(Game game, PlayerManager playerManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            // Builds team info e.g. first UserId set on Blue has PlayerId 0
            // increment by 1 for each added player
            var screenInfo = new LoadScreenInfo(_playerManager.GetPlayers());
            bool pInfo = _game.PacketHandlerManager.sendPacket(peer, screenInfo, Channel.CHL_LOADING_SCREEN);

            // Distributes each players info by UserId
            bool bOk = false;
            foreach (var player in _playerManager.GetPlayers())
            {
                // Giving the UserId in loading screen a name
                var loadName = new LoadScreenPlayerName(player);
                // Giving the UserId in loading screen a champion
                var loadChampion = new LoadScreenPlayerChampion(player);
                bool pName = _game.PacketHandlerManager.sendPacket(peer, loadName, Channel.CHL_LOADING_SCREEN);
                bool pHero = _game.PacketHandlerManager.sendPacket(peer, loadChampion, Channel.CHL_LOADING_SCREEN);

                bOk = (pName && pHero);

                if (!bOk)
                    break;
            }

            return (pInfo && bOk);
        }
    }
}
