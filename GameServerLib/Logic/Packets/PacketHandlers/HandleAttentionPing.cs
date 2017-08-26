using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleAttentionPing : PacketHandlerBase<AttentionPingRequest>
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

        public override bool HandlePacketInternal(Peer peer, AttentionPingRequest data)
        {
            var response = new AttentionPingResponse
            (
                _playerManager.GetPeerInfo(peer),
                data.X,
                data.Y,
                data.TargetNetId,
                data.Type
            );
            var team = _playerManager.GetPeerInfo(peer).Team;
            return _game.PacketHandlerManager.broadcastPacketTeam(team, response, Channel.CHL_S2C);
        }
    }
}
