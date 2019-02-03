using GameServerCore.NetInfo;
using GameServerCore.Packets.Enums;
using PacketDefinitions420.PacketDefinitions.C2S;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class AttentionPingResponse : BasePacket
    {
        public AttentionPingResponse(ClientInfo player, AttentionPingRequest ping) :
            base(PacketCmd.PKT_S2C_ATTENTION_PING)
        {
            Write((float)ping.X);
            Write((float)ping.Y);
            Write((int)ping.TargetNetId);
            WriteNetId(player.Champion);
            Write((byte)ping.Type);
            Write((byte)0xFB); // 4.18
        }
    }
}