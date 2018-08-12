using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SurrenderResult : BasePacket
    {
        public SurrenderResult(bool reason, int yes, int no, TeamId team) 
            : base(PacketCmd.PKT_S2C_SurrenderResult)
        {
            buffer.Write(reason); //surrendererNetworkId
            buffer.Write((byte)yes); //yesVotes
            buffer.Write((byte)no); //noVotes
            buffer.Write((int)team); //team
        }
    }
}