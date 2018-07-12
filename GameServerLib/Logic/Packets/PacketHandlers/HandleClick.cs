using ENet;
using LeagueSandbox.GameServer.Logic.Attributes;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    [DisabledHandler]
    public class HandleClick : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CLICK;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var click = new Click(data);
            var msg = $"Object {PlayerManager.GetPeerInfo(peer).Champion.NetId} clicked on {click.TargetNetId}";
            Logger.LogCoreInfo(msg);

            return true;
        }
    }
}
