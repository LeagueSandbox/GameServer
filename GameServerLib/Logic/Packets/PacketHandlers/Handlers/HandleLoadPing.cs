using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleLoadPing : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_Ping_Load_Info;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleLoadPing(Game game, PlayerManager playerManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var loadInfo = new PingLoadInfo(data);
            var peerInfo = _playerManager.GetPeerInfo(peer);
            if (peerInfo == null)
                return false;
            var response = new PingLoadInfo(loadInfo, peerInfo.UserId);

            //Logging->writeLine("loaded: %f, ping: %f, %f", loadInfo->loaded, loadInfo->ping, loadInfo->f3);
            return _game.PacketHandlerManager.broadcastPacket(response, Channel.CHL_LOW_PRIORITY, PacketFlags.None);
        }
    }
}
