using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleHeartBeat : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_HEART_BEAT;
        public override Channel PacketChannel => Channel.CHLGamePLAY;

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var heartbeat = new HeartBeat(data);

            var diff = heartbeat.AckTime - heartbeat.ReceiveTime;
            if (heartbeat.ReceiveTime > heartbeat.AckTime)
            {
                var peerInfo = PlayerManager.GetPeerInfo(peer);
                var msg = $"Player {peerInfo.UserId} sent an invalid heartbeat - Timestamp error (diff: {diff})";
                Logger.LogCoreWarning(msg);
            }

            return true;
        }
    }
}
