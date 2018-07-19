using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class PingLoadInfoResponse : BasePacket
    {
        public PingLoadInfoResponse(Game game, PingLoadInfoRequest loadInfo, long id) 
            : base(game, PacketCmd.PKT_S2C_PING_LOAD_INFO, loadInfo.NetId)
        {
            Write((uint)loadInfo.Position);
            Write((ulong)id);
            Write(loadInfo.Loaded);
            Write(loadInfo.Unk2);
            Write(loadInfo.Ping);
            Write(loadInfo.Unk3);
            Write(loadInfo.Unk4);
        }
    }
}