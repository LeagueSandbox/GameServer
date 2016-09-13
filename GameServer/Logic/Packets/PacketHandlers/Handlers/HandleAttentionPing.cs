using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleAttentionPing : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var ping = new AttentionPing(data);
            var response = new AttentionPingAns(_playerManager.GetPeerInfo(peer), ping);
            return _game.PacketHandlerManager.broadcastPacketTeam(
                _playerManager.GetPeerInfo(peer).Team,
                response,
                Channel.CHL_S2C
            );
        }
    }
    public enum Pings : byte
    {
        Ping_Default = 0,
        Ping_Danger = 2,
        Ping_Missing = 3,
        Ping_OnMyWay = 4,
        Ping_Assist = 6
    }
}
