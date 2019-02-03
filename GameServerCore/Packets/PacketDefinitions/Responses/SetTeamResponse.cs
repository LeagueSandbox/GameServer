using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    class SetTeamResponse : ICoreResponse
    {
        public IAttackableUnit Unit { get; }
        public TeamId Team{ get; }
        public SetTeamResponse(IAttackableUnit unit, TeamId team)
        {
            Unit = unit;
            Team = team;
        }
    }
}
