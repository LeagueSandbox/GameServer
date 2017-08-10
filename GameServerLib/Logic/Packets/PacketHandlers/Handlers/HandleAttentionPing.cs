using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
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
            return _game.PacketHandlerManager.broadcastPacketTeam(_playerManager.GetPeerInfo(peer).Team, response, Channel.CHL_S2C);
        }
    }

    //TODO: Move to separate file
    public enum Pings : byte
    {
        Ping_Default = 0,
        Ping_Danger = 2,
        Ping_Missing = 3,
        Ping_OnMyWay = 4,
        Ping_Assist = 6
    }
}
