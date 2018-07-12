using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleUseObject : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_USE_OBJECT;
        public override Channel PacketChannel => Channel.CHL_C2_S;
        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var parsedData = new UseObject(data);
            var champion = PlayerManager.GetPeerInfo(peer).Champion;
            var msg = $"Object {champion.NetId} is trying to use (right clicked) {parsedData.TargetNetId}";
            Logger.LogCoreInfo(msg);

            return true;
        }
    }
}
