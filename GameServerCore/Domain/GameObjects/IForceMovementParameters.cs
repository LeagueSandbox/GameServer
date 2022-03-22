using GameServerCore.Enums;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects
{
    public interface IForceMovementParameters
    {
        StatusFlags SetStatus { get; set; }
        /// <summary>
        /// Amount of time passed since the unit started dashing.
        /// </summary>
        float ElapsedTime { get; }
        /// <summary>
        /// Speed to use for the movement.
        /// </summary>
        float PathSpeedOverride { get; }
        /// <summary>
        /// How fast to accelerate downwards.
        /// </summary>
        float ParabolicGravity { get; }
        /// <summary>
        /// Position for the peak of the vertical portion of the movement.
        /// </summary>
        Vector2 ParabolicStartPoint { get; }
        /// <summary>
        /// Whether or not the unit performing the movement should face the direction it had before starting the movement.
        /// </summary>
        bool KeepFacingDirection { get; }
        /// <summary>
        /// NetID of the unit to move towards.
        /// </summary>
        uint FollowNetID { get; }
        /// <summary>
        /// Maximum distance to follow the FollowNetID.
        /// </summary>
        float FollowDistance { get; }
        /// <summary>
        /// Distance ahead of the FollowNetID to travel after reaching it.
        /// </summary>
        float FollowBackDistance { get; }
        /// <summary>
        /// Maximum amount of time to follow the FollowNetID.
        /// </summary>
        float FollowTravelTime { get; }

        void SetTimeElapsed(float time);
    }
}