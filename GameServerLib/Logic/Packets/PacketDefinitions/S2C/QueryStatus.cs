using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class QueryStatus : BasePacket
    {
        public QueryStatus(Game game)
            : base(PacketCmd.PKT_S2C_QUERY_STATUS_ANS)
        {
            Write((byte)1); //ok
        }
    }
}