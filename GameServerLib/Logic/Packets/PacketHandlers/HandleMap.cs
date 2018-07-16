using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleMap : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CLIENT_READY;
        public override Channel PacketChannel => Channel.CHL_LOADING_SCREEN;

        public HandleMap(Game game)
        {
            _game = game;
            _playerManager = game.GetPlayerManager();
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            // Builds team info e.g. first UserId set on Blue has PlayerId 0
            // increment by 1 for each added player
            var screenInfo = new LoadScreenInfo(_game, _playerManager.GetPlayers());
            var pInfo = _game.PacketHandlerManager.SendPacket(peer, screenInfo, Channel.CHL_LOADING_SCREEN);

            // Distributes each players info by UserId
            var bOk = false;
            foreach (var player in _playerManager.GetPlayers())
            {
                // Giving the UserId in loading screen a name
                var loadName = new LoadScreenPlayerName(_game, player);
                // Giving the UserId in loading screen a champion
                var loadChampion = new LoadScreenPlayerChampion(_game, player);
                var pName = _game.PacketHandlerManager.SendPacket(peer, loadName, Channel.CHL_LOADING_SCREEN);
                var pHero = _game.PacketHandlerManager.SendPacket(peer, loadChampion, Channel.CHL_LOADING_SCREEN);

                bOk = pName && pHero;

                if (!bOk)
                {
                    break;
                }
            }

            return pInfo && bOk;
        }
    }
}
