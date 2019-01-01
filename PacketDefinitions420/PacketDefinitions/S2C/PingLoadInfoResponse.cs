using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class PingLoadInfoResponse : BasePacket
    {
        public PingLoadInfoResponse(uint netId, uint clientId, float loaded, float unk2, short ping, short unk3, byte unk4, ulong playerId)
            : base(PacketCmd.PKT_S2C_PING_LOAD_INFO, netId)
        {
            Write((uint)clientId);
            Write((ulong)playerId);
            Write((float)loaded);
            Write((float)unk2);
            Write((short)ping);
            Write((short)unk3);
            Write((byte)unk4);
        }
    }
}