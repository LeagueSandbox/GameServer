using System.Numerics;

namespace GameServerCore.Domain.GameObjects
{
    /// <summary>
    /// Interface for the GameObject of type Particle. The Particle class is used for all in-game visual effects meant to be explicitly networked by the server (never spawned client-side).
    /// </summary>
    public interface IParticle : IGameObject
    {
        /// <summary>
        /// Object which spawned or caused the particle to be instanced
        /// </summary>
        IGameObject Owner { get; }
        /// <summary>
        /// Client-sided, internal name of the particle used in networking, usually always ends in .troy
        /// </summary>
        string Name { get; }
        /// <summary>
        /// GameObject this particle is currently attached to. Null when not attached to anything.
        /// </summary>
        IGameObject TargetObject { get; }
        /// <summary>
        /// Position this object is spawned at. *NOTE*: Does not update. Refer to TargetObject.GetPosition() if particle is supposed to be attached.
        /// </summary>
        Vector2 TargetPosition { get; }
        /// <summary>
        /// Client-sided, internal name of the bone that this particle should be attached to for networking
        /// </summary>
        string BoneName { get; }
        /// <summary>
        /// Scale of the particle used in networking
        /// </summary>
        float Scale { get; }
        /// <summary>
        /// Total game-time that this particle should exist for
        /// </summary>
        float Lifetime { get; }
        /// <summary>
        /// Whether or not the particle should be affected by vision,
        /// false = always visible,
        /// true = visibility can be obstructed
        /// </summary>
        bool VisionAffected { get; }

        /// <summary>
        /// Returns the total game-time passed since the particle was added to ObjectManager
        /// </summary>
        float GetTimeAlive();
    }
}
