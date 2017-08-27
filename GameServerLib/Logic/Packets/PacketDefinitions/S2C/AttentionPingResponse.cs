using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AttentionPingResponse : BasePacket
    {
        public AttentionPingResponse(AttentionPingResponseArgs args)
            : base(PacketCmd.PKT_S2C_AttentionPing)
        {
            buffer.Write((float)args.X);
            buffer.Write((float)args.Y);
            buffer.Write((uint)args.TargetNetId);
            buffer.Write((uint)args.PlayerChampionNetId);
            buffer.Write((byte)args.Type);
            buffer.Write((byte)0xFB); // 4.18
        }
    }
}