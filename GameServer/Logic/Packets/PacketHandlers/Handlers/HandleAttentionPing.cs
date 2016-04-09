using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using static ENet.Native;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleAttentionPing : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            var ping = new AttentionPing(data);
            var response = new AttentionPingAns(game.getPeerInfo(peer), ping);
            return PacketHandlerManager.getInstace().broadcastPacketTeam(game.getPeerInfo(peer).getTeam(), response, Channel.CHL_S2C);
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
