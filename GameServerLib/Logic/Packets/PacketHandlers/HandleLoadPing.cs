using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleLoadPing : PacketHandlerBase
    {
        private readonly IPacketReader _packetReader;
        private readonly IPacketNotifier _packetNotifier;
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_PING_LOAD_INFO;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleLoadPing(Game game)
        {
            _packetReader = game.PacketReader;
            _packetNotifier = game.PacketNotifier;
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _packetReader.ReadPingLoadInfoRequest(data);
            var peerInfo = _playerManager.GetPeerInfo(peer);
            if (peerInfo == null)
            {
                return false;
            }

            _packetNotifier.NotifyPingLoadInfo(request, peerInfo.UserId);
            return true;
        }
    }
}
