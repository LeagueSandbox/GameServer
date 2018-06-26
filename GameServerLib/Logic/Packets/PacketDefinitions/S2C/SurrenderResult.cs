using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SurrenderResult : BasePacket
    {
        public SurrenderResult(bool reason, int yes, int no, TeamId team) 
            : base(PacketCmd.PKT_S2_C_SURRENDER_RESULT)
        {
            _buffer.Write(reason); //surrendererNetworkId
            _buffer.Write((byte)yes); //yesVotes
            _buffer.Write((byte)no); //noVotes
            _buffer.Write((int)team); //team
        }
    }
}