using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class EnterVision2Response : ICoreResponse
    {
        public IGameObject Obj { get; }
        public TeamId Team { get; }
        public EnterVision2Response(IGameObject o, TeamId team)
        {
            Obj = o;
            Team = team;
        }
    }
}