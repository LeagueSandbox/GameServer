using System.Numerics;

namespace GameServerCore.Domain.GameObjects
{
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
        /// Client-sided, internal name of the bone that this particle should be attached to for networking
        /// </summary>
        string BoneName { get; }
        /// <summary>
        /// Scale of the particle used in networking
        /// </summary>
        float Scale { get; }
        /// <summary>
        /// 3 dimensional forward vector (where the particle faces) used in networking
        /// </summary>
        Vector3 Direction { get; }
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
