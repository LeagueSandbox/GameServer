using GameServerCore.Enums;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects
{
    /// <summary>
    /// Interface for a Region. The Region class is used for extranneous vision and collision effects.
    /// </summary>
    public interface IRegion : IGameObject
    {
        int Type { get; }
        IGameObject CollisionUnit { get; }
        int OwnerClientID { get; }
        uint VisionNetID { get; }
        uint VisionBindNetID { get; }
        /// <summary>
        /// Total game-time that this region should exist for
        /// </summary>
        float Lifetime { get; }
        float GrassRadius { get; }
        /// <summary>
        /// Scale of the region used in networking
        /// </summary>
        float Scale { get; }
        float AdditionalSize { get; }
        bool HasCollision { get; }
        bool GrantVision { get; }
        bool RevealsStealth { get; }
    }
}