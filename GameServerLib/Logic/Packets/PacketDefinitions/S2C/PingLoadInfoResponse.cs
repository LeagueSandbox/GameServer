using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class PingLoadInfoResponse : BasePacket
    {
        public PingLoadInfoResponse(PingLoadInfoRequest loadInfo, long id) : base(PacketCmd.PKT_S2C_Ping_Load_Info, loadInfo.netId)
        {
            buffer.Write((uint)loadInfo.position);
            buffer.Write((ulong)id);
            buffer.Write((float)loadInfo.loaded);
            buffer.Write((float)loadInfo.unk2);
            buffer.Write((short)loadInfo.ping);
            buffer.Write((short)loadInfo.unk3);
            buffer.Write((byte)loadInfo.unk4);
        }
    }
}