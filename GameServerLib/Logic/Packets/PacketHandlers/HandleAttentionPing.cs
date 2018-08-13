using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleAttentionPing : PacketHandlerBase
    {
        private readonly IPacketReader _packetReader;
        private readonly IPacketNotifier _packetNotifier;
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_ATTENTION_PING;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleAttentionPing(Game game)
        {
            _packetReader = game.PacketReader;
            _packetNotifier = game.PacketNotifier;
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _packetReader.ReadAttentionPingRequest(data);
            var client = _playerManager.GetPeerInfo(peer);
            _packetNotifier.NotifyPing(client, request.X, request.Y, request.TargetNetId, Pings.PING_DANGER);
            return true;
        }
    }
}
