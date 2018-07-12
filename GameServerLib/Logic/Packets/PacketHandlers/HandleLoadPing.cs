using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleLoadPing : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_PING_LOAD_INFO;
        public override Channel PacketChannel => Channel.CHL_C2_S;
        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var loadInfo = new PingLoadInfoRequest(data);
            var peerInfo = PlayerManager.GetPeerInfo(peer);
            if (peerInfo == null)
            {
                return false;
            }
            var response = new PingLoadInfoResponse(loadInfo, peerInfo.UserId);

            //Logging->writeLine("loaded: %f, ping: %f, %f", loadInfo->loaded, loadInfo->ping, loadInfo->f3);
            return Game.PacketHandlerManager.BroadcastPacket(response, Channel.CHL_LOW_PRIORITY, PacketFlags.None);
        }
    }
}
