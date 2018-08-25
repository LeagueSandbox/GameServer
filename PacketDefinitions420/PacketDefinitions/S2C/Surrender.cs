using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class Surrender : BasePacket
    {
        public Surrender(IAttackableUnit starter, byte flag, byte yesVotes, byte noVotes, byte maxVotes, TeamId team, float timeOut)
            : base(PacketCmd.PKT_S2C_SURRENDER)
        {
            Write(flag); // Flag. 2 bits
            WriteNetId(starter);
            Write(yesVotes);
            Write(noVotes);
            Write(maxVotes);
            Write((int)team);
            Write(timeOut);
        }
    }
}