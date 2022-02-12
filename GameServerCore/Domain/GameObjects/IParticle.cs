using GameServerCore.Enums;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects
{
    /// <summary>
    /// Interface for the GameObject of type Particle. The Particle class is used for all in-game visual effects meant to be explicitly networked by the server (never spawned client-side).
    /// </summary>
    public interface IParticle : IGameObject
    {
        /// <summary>
        /// Creator of this particle.
        /// </summary>
        IGameObject Caster { get; }
        /// <summary>
        /// Primary bind object.
        /// </summary>
        IGameObject BindObject { get; }
        /// <summary>
        /// Client-sided, internal name of the particle used in networking, usually always ends in .troy
        /// </summary>
        string Name { get; }
        /// <summary>
        /// GameObject this particle is currently attached to. Null when not attached to anything.
        /// </summary>
        IGameObject TargetObject { get; }
        /// <summary>
        /// Position this object is spawned at.
        /// </summary>
        Vector2 StartPosition { get; }
        /// <summary>
        /// Position this object is aimed at and/or moving towards.
        /// </summary>
        Vector2 EndPosition { get; }
        /// <summary>
        /// Client-sided, internal name of the bone that this particle should be attached to for networking
        /// </summary>
        string BoneName { get; }
        /// <summary>
        /// Client-sided, internal name of the bone that this particle should be attached to on the target, for networking.
        /// </summary>
        string TargetBoneName { get; }
        /// <summary>
        /// Scale of the particle used in networking
        /// </summary>
        float Scale { get; }
        /// <summary>
        /// Total game-time that this particle should exist for
        /// </summary>
        float Lifetime { get; }
        /// <summary>
        /// The only team that should be able to see this particle.
        /// </summary>
        TeamId SpecificTeam { get; }
        /// <summary>
        /// The only unit that should be able to see this particle.
        /// Only effective if this is a player controlled unit.
        /// </summary>
        IGameObject SpecificUnit { get; }
        /// <summary>
        /// Whether or not the particle should be titled along the ground towards its end position.
        /// Effectively uses the ground height for the end position.
        /// </summary>
        bool FollowsGroundTilt { get; }
        /// <summary>
        /// Flags which determine how the particle behaves. Values unknown.
        /// </summary>
        FXFlags Flags { get; }

        /// <summary>
        /// Returns the total game-time passed since the particle was added to ObjectManager
        /// </summary>
        float GetTimeAlive();
    }
}
