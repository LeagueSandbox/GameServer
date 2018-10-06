using GameServerCore;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;


namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleAttentionPing : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_ATTENTION_PING;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleAttentionPing(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, byte[] data)
        {
            var request = _game.PacketReader.ReadAttentionPingRequest(data);
            var client = _playerManager.GetPeerInfo(userId);
            _game.PacketNotifier.NotifyPing(client, request.X, request.Y, request.TargetNetId, request.Type);
            return true;
        }
    }
}
