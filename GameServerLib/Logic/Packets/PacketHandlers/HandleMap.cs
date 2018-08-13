using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleMap : PacketHandlerBase
    {
        private readonly IPacketNotifier _packetNotifier;
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CLIENT_READY;
        public override Channel PacketChannel => Channel.CHL_LOADING_SCREEN;

        public HandleMap(Game game)
        {
            _packetNotifier = game.PacketNotifier;
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            // Builds team info e.g. first UserId set on Blue has PlayerId 0
            // increment by 1 for each added player
            _packetNotifier.NotifyLoadScreenInfo(peer, _playerManager.GetPlayers());

            // Distributes each players info by UserId
            foreach (var player in _playerManager.GetPlayers())
            {
                // Giving the UserId in loading screen a name
                _packetNotifier.NotifyLoadScreenPlayerName(peer, player);
                // Giving the UserId in loading screen a champion
                _packetNotifier.NotifyLoadScreenPlayerChampion(peer, player);
            }

            return true;
        }
    }
}
