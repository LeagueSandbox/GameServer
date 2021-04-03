using System.Numerics;

namespace GameServerCore.Domain.GameObjects
{
    public interface IForceMovementParameters
    {
        public float PathSpeedOverride { get; }
        public float ParabolicGravity { get; }
        public Vector2 ParabolicStartPoint { get; }
        public bool KeepFacingDirection { get; }
        public uint FollowNetID { get; }
        public float FollowDistance { get; }
        public float FollowBackDistance { get; }
        public float FollowTravelTime { get; }
    }
}