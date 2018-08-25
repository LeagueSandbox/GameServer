using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SetTeam : BasePacket
    {
        public SetTeam(IAttackableUnit unit, TeamId team) : base(PacketCmd.PKT_S2C_SET_TEAM)
        {
            WriteNetId(unit);
            Write((int)team);
        }
    }
}