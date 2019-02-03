using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class DashResponse : ICoreResponse
    {
        public IAttackableUnit Unit { get; }
        public ITarget Target { get; }
        public float DashSpeed { get; }
        public bool KeepFacingLastDirection { get; }
        public float LeapHeight { get; }
        public float FollowTargetMaxDistance { get; }
        public float BackDistance { get; }
        public float TravelTime { get; }
        public DashResponse(IAttackableUnit u, ITarget t, float dashSpeed, bool keepFacingLastDirection, float leapHeight, float followTargetMaxDistance, float backDistance, float travelTime)
        {
            Unit = u;
            Target = t;
            DashSpeed = dashSpeed;
            KeepFacingLastDirection = keepFacingLastDirection;
            LeapHeight = leapHeight;
            FollowTargetMaxDistance = followTargetMaxDistance;
            BackDistance = backDistance;
            TravelTime = travelTime;
        }
    }
}