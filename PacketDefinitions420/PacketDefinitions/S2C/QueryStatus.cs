using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class QueryStatus : BasePacket
    {
        public QueryStatus()
            : base(PacketCmd.PKT_S2C_QUERY_STATUS_ANS)
        {
            Write((byte)1); //ok
        }
    }
}