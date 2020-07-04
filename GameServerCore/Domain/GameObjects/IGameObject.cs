using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IGameObject : ITarget, IUpdate
    {
        /// <summary>
        ///  Identifier unique to this game object.
        /// </summary>
        uint NetId { get; }
        /// <summary>
        /// Waypoints that make up the path a game object is walking in.
        /// </summary>
        List<Vector2> Waypoints { get; }
        /// <summary>
        /// Index of the waypoint in the list of waypoints that the object is current on.
        /// </summary>
        int WaypointIndex { get; }
        /// <summary>
        /// Current target the game object is looking/attacking/moving to (can be coordinates or an object).
        /// </summary>
        ITarget Target { get; }
        /// <summary>
        /// Team identifier, refer to TeamId enum.
        /// </summary>
        TeamId Team { get; }
        /// <summary>
        /// Radius of the circle which is used for collision detection between objects or terrain.
        /// </summary>
        float CollisionRadius { get; }
        /// <summary>
        /// Radius of the circle which is used for vision; detecting if objects are visible given terrain, and if so, networked to the player (or team) that owns this game object.
        /// </summary>
        float VisionRadius { get; }
        /// <summary>
        /// Used to synchronize movement between client and server. Is currently assigned Env.TickCount.
        /// </summary>
        uint SyncId { get; }
        /// <summary>
        /// Refers to the height that the object is at in 3D space. *NOTE* Should be renamed.
        /// </summary>
        // TODO: Change this to property
        float GetZ();
        /// <summary>
        /// Returns the units that the game object travels each second. Default 0 unless overriden.
        /// </summary>
        float GetMoveSpeed();

        /// <summary>
        /// Called every update after the object sets its waypoints.
        /// </summary>
        void ClearMovementUpdated();

        /// <summary>
        /// Called by ObjectManager after AddObject.
        /// </summary>
        void OnAdded();
        /// <summary>
        /// Called when the object is ontop of another object or when the object is inside terrain.
        /// </summary>
        void OnCollision(IGameObject collider);
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
        void SetTeam(TeamId team);
        /// <summary>
        /// Will cause ObjectManager to remove the object (usually) both server and client-side next update.
        /// </summary>
        void SetToRemove();
        /// <summary>
        /// Will force the object to be networked to the specified team.
        /// </summary>
        void SetVisibleByTeam(TeamId team, bool visible);
        /// <summary>
        /// Returns the waypoint that the object is currently moving to.
        /// </summary>
        Vector2 GetNextWaypoint();
        /// <summary>
        /// Returns the vector which represents the 2d orientation that the object is moving in.
        /// </summary>
        Vector2 GetDirection();
        /// <summary>
        /// Whether or not the object has reached its final waypoint.
        /// </summary>
        bool IsPathEnded();
        /// <summary>
        /// Sets the object's path to the newWaypoints
        /// </summary>
        /// <param name="newWaypoints">New path of Vector2 coordinates that the unit will move to.</param>
        void SetWaypoints(List<Vector2> newWaypoints);

        /// <summary>
        /// Whether or not the specified object is colliding with this object.
        /// </summary>
        /// <param name="o">An object that could be colliding with this object.</param>
        bool IsCollidingWith(IGameObject o);
        /// <summary>
        /// Whether or not the object has changed its waypoints since the last game update.
        /// </summary>
        bool IsMovementUpdated();
        /// <summary>
        /// Whether or not the object should be removed from the game (usually both server and client-side). Refer to ObjectManager.
        /// </summary>
        bool IsToRemove();
        /// <summary>
        /// Whether or not the object is networked to a specified team.
        /// </summary>
        /// <param name="team">A team which could have vision of this object.</param>
        bool IsVisibleByTeam(TeamId team);
    }
}
