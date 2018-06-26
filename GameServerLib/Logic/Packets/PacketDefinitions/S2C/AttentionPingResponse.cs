using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AttentionPingResponse : BasePacket
    {
        public AttentionPingResponse(ClientInfo player, AttentionPingRequest ping) : base(PacketCmd.PKT_S2_C_ATTENTION_PING)
        {
            _buffer.Write((float)ping.X);
            _buffer.Write((float)ping.Y);
            _buffer.Write((int)ping.TargetNetId);
            _buffer.Write((int)player.Champion.NetId);
            _buffer.Write((byte)ping.Type);
            _buffer.Write((byte)0xFB); // 4.18
        }
    }
}