using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleAttentionPing : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_AttentionPing;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleAttentionPing(Game game, PlayerManager playerManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var ping = new AttentionPing(data);
            var response = new AttentionPingAns(_playerManager.GetPeerInfo(peer), ping);
            var team = _playerManager.GetPeerInfo(peer).Team;
            return _game.PacketHandlerManager.broadcastPacketTeam(team, response, Channel.CHL_S2C);
        }
    }
}
