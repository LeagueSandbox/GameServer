using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class LeaveVisionResponse : ICoreResponse
    {
        public IGameObject Obj { get; }
        public TeamId Team { get; }
        public LeaveVisionResponse(IGameObject o, TeamId team)
        {
            Obj = o;
            Team = team;
        }
    }
}