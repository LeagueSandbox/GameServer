using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleAttentionPing : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_ATTENTION_PING;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var ping = new AttentionPingRequest(data);
            var response = new AttentionPingResponse(PlayerManager.GetPeerInfo(peer), ping);
            var team = PlayerManager.GetPeerInfo(peer).Team;
            return Game.PacketHandlerManager.BroadcastPacketTeam(team, response, Channel.CHL_S2_C);
        }
    }
}
