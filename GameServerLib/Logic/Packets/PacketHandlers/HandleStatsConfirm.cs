using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleStatsConfirm : PacketHandlerBase<EmptyClientPacket>
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_StatsConfirm;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public override bool HandlePacketInternal(Peer peer, EmptyClientPacket data)
        {
            return true;
        }
    }
}
