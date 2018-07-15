using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AttentionPingResponse : BasePacket
    {
        public AttentionPingResponse(ClientInfo player, AttentionPingRequest ping) : base(PacketCmd.PKT_S2C_ATTENTION_PING)
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