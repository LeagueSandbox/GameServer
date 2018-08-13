using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class PingLoadInfoResponse : BasePacket
    {
        public PingLoadInfoResponse(uint netId, uint position, float loaded, float unk2, short ping, short unk3, byte unk4, long id)
            : base(PacketCmd.PKT_S2C_PING_LOAD_INFO, netId)
        {
            Write((uint)position);
            Write((ulong)id);
            Write((float)loaded);
            Write((float)unk2);
            Write((short)ping);
            Write((short)unk3);
            Write((byte)unk4);
        }
    }
}