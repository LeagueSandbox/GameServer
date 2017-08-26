using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AttentionPingResponse : BasePacket
    {
        public AttentionPingResponse(ClientInfo player, float x, float y, int targetNetId, Pings type)
            : base(PacketCmd.PKT_S2C_AttentionPing)
        {
            buffer.Write((float)x);
            buffer.Write((float)y);
            buffer.Write((int)targetNetId);
            buffer.Write((int)player.Champion.NetId);
            buffer.Write((byte)type);
            buffer.Write((byte)0xFB); // 4.18
        }
    }
}