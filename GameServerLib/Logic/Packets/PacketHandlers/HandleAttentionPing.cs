using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleAttentionPing : PacketHandlerBase<AttentionPingRequest>
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;
        private readonly IPacketArgsTranslationService _translationService;
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_AttentionPing;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleAttentionPing(Game game, PlayerManager playerManager,
            IPacketArgsTranslationService translationService)
        {
            _game = game;
            _playerManager = playerManager;
            _translationService = translationService;
        }

        public override bool HandlePacketInternal(Peer peer, AttentionPingRequest data)
        {
            var client = _playerManager.GetPeerInfo(peer);
            var team = client.Team;

            var x = data.X;
            var y = data.Y;
            var target = data.TargetNetId;
            var args = _translationService.TranslateAttentionPingResponse(client, x, y, target, data.Type);
            var response = new AttentionPingResponse(args);

            return _game.PacketHandlerManager.broadcastPacketTeam(team, response, Channel.CHL_S2C);
        }
    }
}
