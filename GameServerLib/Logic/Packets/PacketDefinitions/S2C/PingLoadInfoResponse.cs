using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class PingLoadInfoResponse : BasePacket
    {
        public PingLoadInfoResponse(PingLoadInfoRequest loadInfo, long id) 
            : base(PacketCmd.PKT_S2C_Ping_Load_Info, loadInfo.NetId)
        {
            buffer.Write((uint)loadInfo.Position);
            buffer.Write((ulong)id);
            buffer.Write((float)loadInfo.Loaded);
            buffer.Write((float)loadInfo.Unk2);
            buffer.Write((short)loadInfo.Ping);
            buffer.Write((short)loadInfo.Unk3);
            buffer.Write((byte)loadInfo.Unk4);
        }
    }
}