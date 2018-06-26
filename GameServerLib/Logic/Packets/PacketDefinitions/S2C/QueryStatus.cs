using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class QueryStatus : BasePacket
    {
        public QueryStatus() 
            : base(PacketCmd.PKT_S2_C_QUERY_STATUS_ANS)
        {
            _buffer.Write((byte)1); //ok
        }
    }
}