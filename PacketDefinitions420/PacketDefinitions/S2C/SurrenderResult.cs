using GameServerCore.Enums;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SurrenderResult : BasePacket
    {
        public SurrenderResult(bool reason, int yes, int no, TeamId team)
            : base(PacketCmd.PKT_S2C_SURRENDER_RESULT)
        {
            Write(reason); //surrendererNetworkId
            Write((byte)yes); //yesVotes
            Write((byte)no); //noVotes
            Write((int)team); //team
        }
    }
}