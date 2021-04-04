using GameServerCore.Domain.GameObjects;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits
{
    public class ForceMovementParameters : IForceMovementParameters
    {
        public float PathSpeedOverride { get; set; }
        public float ParabolicGravity { get; set; }
        public Vector2 ParabolicStartPoint { get; set; }
        public bool KeepFacingDirection { get; set; }
        public uint FollowNetID { get; set; }
        public float FollowDistance { get; set; }
        public float FollowBackDistance { get; set; }
        public float FollowTravelTime { get; set; }
    }
}
