using GameServerCore.Enums;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits
{
    public class ForceMovementParameters
    {
        /// <summary>
        /// Status flags which are disabled while dashing.
        /// </summary>
        public StatusFlags SetStatus { get; set; } = StatusFlags.CanAttack | StatusFlags.CanCast | StatusFlags.CanMove;
        /// <summary>
        /// Amount of time passed since the unit started dashing.
        /// </summary>
        public float ElapsedTime { get; set; }
        /// <summary>
        /// The distance traveled from the beginning of the dash.
        /// </summary>
        public float PassedDistance { get; set; }
        /// <summary>
        /// Speed to use for the movement.
        /// </summary>
        public float PathSpeedOverride { get; set; }
        /// <summary>
        /// Maximum vertical height.
        /// NOTES: Internally follows the path of a parabola, stretched in the x and y axis by the distance to the destination and stretched in the z axis to the maximum height (this stretching of the z axis scales the vertical speed).
        /// </summary>
        public float ParabolicGravity { get; set; }
        /// <summary>
        /// End position of the movement.
        /// </summary>
        public Vector2 ParabolicStartPoint { get; set; }
        /// <summary>
        /// Whether or not the unit performing the movement should face the direction it had before starting the movement.
        /// </summary>
        public bool KeepFacingDirection { get; set; }
        /// <summary>
        /// NetID of the unit to move towards.
        /// </summary>
        public uint FollowNetID { get; set; }
        /// <summary>
        /// Maximum distance to follow the FollowNetID.
        /// </summary>
        public float FollowDistance { get; set; }
        /// <summary>
        /// Distance ahead of the FollowNetID to travel after reaching it.
        /// </summary>
        public float FollowBackDistance { get; set; }
        /// <summary>
        /// Maximum amount of time to follow the FollowNetID.
        /// </summary>
        public float FollowTravelTime { get; set; }
    }
}
