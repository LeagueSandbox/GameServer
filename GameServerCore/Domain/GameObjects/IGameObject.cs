using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    /// <summary>
    /// Base class for all objects in League of Legends.
    /// GameObjects normally follow these guidelines of functionality: Position, Collision, Vision, Team, and Networking.
    /// </summary>
    public interface IGameObject : IUpdate
    {
        // Structure follows hierarchy of features (ex: an object is defined as collide-able before walk-able)

        /// <summary>
        ///  Identifier unique to this game object.
        /// </summary>
        uint NetId { get; }
        /// <summary>
        /// Radius of the circle which is used for collision detection between objects or terrain.
        /// </summary>
        float CollisionRadius { get; }
        /// <summary>
        /// Radius of the circle which is used for pathfinding around objects and terrain.
        /// </summary>
        float PathfindingRadius { get; }
        /// <summary>
        /// Position of this GameObject from a top-down view.
        /// </summary>
        Vector2 Position { get; }
        /// <summary>
        /// 3D orientation of this GameObject (based on ground-level).
        /// </summary>
        Vector3 Direction { get; }
        /// <summary>
        /// Used to synchronize movement between client and server. Is currently assigned Env.TickCount.
        /// </summary>
        int SyncId { get; }
        /// <summary>
        /// Team identifier, refer to TeamId enum.
        /// </summary>
        TeamId Team { get; }
        /// <summary>
        /// Radius of the circle which is used for vision; detecting if objects are visible given terrain, and if so, networked to the player (or team) that owns this game object.
        /// </summary>
        float VisionRadius { get; }

        /// <summary>
        /// Called by ObjectManager after AddObject (usually right after instatiation of GameObject).
        /// </summary>
        void OnAdded();

        /// <summary>
        /// Whether or not the object should be removed from the game (usually both server and client-side). Refer to ObjectManager.
        /// </summary>
        bool IsToRemove();

        /// <summary>
        /// Will cause ObjectManager to remove the object (usually) both server-side and client-side next update.
        /// </summary>
        void SetToRemove();

        /// <summary>
        /// Called by ObjectManager after the object has been SetToRemove.
        /// </summary>
        void OnRemoved();

        /// <summary>
        /// Sets the server-sided position of this object.
        /// </summary>
        void SetPosition(float x, float y);

        /// <summary>
        /// Sets the server-sided position of this object.
        /// </summary>
        void SetPosition(Vector2 vec);

        /// <summary>
        /// Sets the collision radius of this GameObject.
        /// </summary>
        /// <param name="newRadius">Radius to set.</param>
        void SetCollisionRadius(float newRadius);

        /// <summary>
        /// Refers to the height that the object is at in 3D space.
        /// </summary>
        float GetHeight();

        /// <summary>
        /// Gets the position of this GameObject in 3D space, where the Y value represents height.
        /// Mostly used for packets.
        /// </summary>
        /// <returns>Vector3 position.</returns>
        Vector3 GetPosition3D();

        /// <summary>
        /// Whether or not the specified object is colliding with this object.
        /// </summary>
        /// <param name="o">An object that could be colliding with this object.</param>
        bool IsCollidingWith(IGameObject o);

        /// <summary>
        /// Called by ObjectManager when the object is ontop of another object or when the object is inside terrain.
        /// </summary>
        void OnCollision(IGameObject collider, bool isTerrain = false);

        /// <summary>
        /// Sets the object's team.
        /// </summary>
        /// <param name="team">TeamId.BLUE/PURPLE/NEUTRAL</param>
        void SetTeam(TeamId team);

        /// <summary>
        /// Sets this GameObject's current orientation (only X and Z are used in movement).
        /// </summary>
        void FaceDirection(Vector3 newDirection, bool isInstant = true, float turnTime = 0.08333f);

        /// <summary>
        /// Whether or not the object is within the vision of the specified team.
        /// </summary>
        /// <param name="team">A team which could have vision of this object.</param>
        bool IsVisibleByTeam(TeamId team);

        /// <summary>
        /// Sets the object as visible to a specified team.
        /// Should be called in the ObjectManager. By itself, it only affects the return value of IsVisibleByTeam.
        /// </summary>
        /// <param name="team">A team which could have vision of this object.</param>
        /// <param name="visible">New value.</param>
        void SetVisibleByTeam(TeamId team, bool visible = true);
        
        /// <summary>
        /// Whether or not the object is visible for the specified player.
        /// <summary>
        /// <param name="userId">The player in relation to which the value is obtained</param>
        bool IsVisibleForPlayer(int userId);

        /// <summary>
        /// Sets the object as visible and or not to a specified player.
        /// Should be called in the ObjectManager. By itself, it only affects the return value of IsVisibleForPlayer.
        /// <summary>
        /// <param name="userId">The player for which the value is set.</param>
        /// <param name="visible">New value.</param>
        void SetVisibleForPlayer(int userId, bool visible = true);

        /// <summary>
        /// Whether or not the object is spawned on the player's client side.
        /// <summary>
        /// <param name="userId">The player in relation to which the value is obtained</param>
        bool IsSpawnedForPlayer(int userId);

        /// <summary>
        /// Sets the object as spawned on the player's client side.
        /// Should be called in the ObjectManager. By itself, it only affects the return value of IsSpawnedForPlayer.
        /// <summary>
        /// <param name="userId">The player for which the value is set.</param>
        void SetSpawnedForPlayer(int userId);

        /// <summary>
        /// Allows to iterate all players who see the object 
        /// </summary>
        IEnumerable<int> VisibleForPlayers { get; }

        /// <summary>
        /// Sets the position of this GameObject to the specified position.
        /// </summary>
        /// <param name="x">X coordinate to set.</param>
        /// <param name="y">Y coordinate to set.</param>
        void TeleportTo(float x, float y);
        /// <summary>
        /// Forces this GameObject to perform the given internally named animation.
        /// </summary>
        /// <param name="animName">Internal name of an animation to play.</param>
        /// <param name="timeScale">How fast the animation should play. Default 1x speed.</param>
        /// <param name="startTime">Time in the animation to start at.</param>
        /// TODO: Verify if this description is correct, if not, correct it.
        /// <param name="speedScale">How much the speed of the GameObject should affect the animation.</param>
        /// <param name="flags">Animation flags. Refer to AnimationFlags enum.</param>
        void PlayAnimation(string animName, float timeScale = 1.0f, float startTime = 0, float speedScale = 0, AnimationFlags flags = 0);
        /// <summary>
        /// Forces the GameObject's current animations to pause/unpause.
        /// </summary>
        /// <param name="pause">Whether or not to pause/unpause animations.</param>
        void PauseAnimation(bool pause);
        /// <summary>
        /// Forces this GameObject to stop playing the specified animation (or optionally all animations) with the given parameters.
        /// </summary>
        /// <param name="animation">Internal name of the animation to stop playing. Set blank/null if stopAll is true.</param>
        /// <param name="stopAll">Whether or not to stop all animations. Only works if animation is empty/null.</param>
        /// <param name="fade">Whether or not the animation should fade before stopping.</param>
        /// <param name="ignoreLock">Whether or not locked animations should still be stopped.</param>
        void StopAnimation(string animation, bool stopAll = false, bool fade = false, bool ignoreLock = true);
    }
}
