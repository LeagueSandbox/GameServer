using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleAttentionPing : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_ATTENTION_PING;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public HandleAttentionPing(Game game, PlayerManager playerManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var ping = new AttentionPingRequest(data);
            var response = new AttentionPingResponse(_playerManager.GetPeerInfo(peer), ping);
            var team = _playerManager.GetPeerInfo(peer).Team;
            return _game.PacketHandlerManager.BroadcastPacketTeam(team, response, Channel.CHL_S2_C);
        }
    }
}
