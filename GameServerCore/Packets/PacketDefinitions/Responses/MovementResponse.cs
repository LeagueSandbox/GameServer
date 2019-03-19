using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class MovementResponse : ICoreResponse
    {
        public IGameObject Obj { get; }
        public MovementResponse(IGameObject o)
        {
            Obj = o;
        }
    }
};