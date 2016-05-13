using ENet;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleAttentionPing : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var ping = new AttentionPing(data);
            var response = new AttentionPingAns(game.GetPeerInfo(peer), ping);
            return game.PacketHandlerManager.broadcastPacketTeam(game.GetPeerInfo(peer).GetTeam(), response, Channel.CHL_S2C);
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
