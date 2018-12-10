using GameServerCore.Domain.GameObjects;
using System.Numerics;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class FaceDirectionResponse : ICoreResponse
    {
        public IAttackableUnit Unit { get; }
        public Vector2 Direction { get; }
        public bool IsInstant { get; }
        public float TurnTime { get; }
        public FaceDirectionResponse(IAttackableUnit u, Vector2 direction, bool isInstant = true, float turnTime = 0.0833F)
        {
            Unit = u;
            Direction = direction;
            IsInstant = isInstant;
            TurnTime = turnTime;
        }
    }
}