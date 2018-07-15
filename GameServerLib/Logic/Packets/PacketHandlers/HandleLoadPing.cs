using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleLoadPing : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_PING_LOAD_INFO;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public HandleLoadPing(Game game, PlayerManager playerManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var loadInfo = new PingLoadInfoRequest(data);
            var peerInfo = _playerManager.GetPeerInfo(peer);
            if (peerInfo == null)
            {
                return false;
            }
            var response = new PingLoadInfoResponse(loadInfo, peerInfo.UserId);

            //Logging->writeLine("loaded: %f, ping: %f, %f", loadInfo->loaded, loadInfo->ping, loadInfo->f3);
            return _game.PacketHandlerManager.BroadcastPacket(response, Channel.CHL_LOW_PRIORITY, PacketFlags.None);
        }
    }
}
