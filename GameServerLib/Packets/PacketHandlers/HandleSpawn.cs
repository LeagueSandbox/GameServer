using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Inventory;
using LeagueSandbox.GameServer.Logging;
using log4net;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSpawn : PacketHandlerBase<SpawnRequest>
    {
        private static ILog _logger = LoggerProvider.GetLogger();
        private readonly Game _game;
        private readonly ItemManager _itemManager;
        private readonly PlayerManager _playerManager;
        private readonly NetworkIdManager _networkIdManager;

        public HandleSpawn(Game game)
        {
            _game = game;
            _itemManager = game.ItemManager;
            _playerManager = game.PlayerManager;
            _networkIdManager = game.NetworkIdManager;
        }

        public override bool HandlePacket(int userId, SpawnRequest req)
        {
            _logger.Debug("Spawning map");
            _game.PacketNotifier.NotifyS2C_StartSpawn(userId);

            var userInfo = _playerManager.GetPeerInfo(userId);
            var om = _game.ObjectManager as ObjectManager;
            if (_game.IsRunning)
            {
                om.OnReconnect(userId, userInfo.Team);
            }
            else
            {
                om.SpawnObjects(userInfo);
            }

            _game.PacketNotifier.NotifySpawnEnd(userId);
            return true;
        }
    }
}