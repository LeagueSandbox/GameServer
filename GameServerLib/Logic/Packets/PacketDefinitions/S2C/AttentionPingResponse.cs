using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AttentionPingResponse : BasePacket
    {
        public AttentionPingResponse(ClientInfo player, AttentionPingRequest ping) : base(PacketCmd.PKT_S2C_AttentionPing)
        {
            buffer.Write((float)ping.x);
            buffer.Write((float)ping.y);
            buffer.Write((int)ping.targetNetId);
            buffer.Write((int)player.Champion.NetId);
            buffer.Write((byte)ping.type);
            buffer.Write((byte)0xFB); // 4.18
        }
    }
}