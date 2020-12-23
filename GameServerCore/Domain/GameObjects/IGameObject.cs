using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    /// <summary>
    /// Base class for all objects in League of Legends.
    /// </summary>
    public interface IGameObject : ITarget, IUpdate
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
        /// Waypoints that make up the path a game object is walking in.
        /// </summary>
        List<Vector2> Waypoints { get; }
        /// <summary>
        /// Index of the waypoint in the list of waypoints that the object is current on.
        /// </summary>
        int WaypointIndex { get; }
        /// <summary>
        /// Used to synchronize movement between client and server. Is currently assigned Env.TickCount.
        /// </summary>
        uint SyncId { get; }
        /// <summary>
        /// Team identifier, refer to TeamId enum.
        /// </summary>
        TeamId Team { get; }
        /// <summary>
        /// Radius of the circle which is used for vision; detecting if objects are visible given terrain, and if so, networked to the player (or team) that owns this game object.
        /// </summary>
        float VisionRadius { get; }
        /// <summary>
        /// Current target the game object is looking at, moving to, or attacking (can be coordinates or an object)
        /// </summary>
        ITarget Target { get; }

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
        /// Refers to the height that the object is at in 3D space.
        /// </summary>
        float GetHeight();

        /// <summary>
        /// Whether or not the specified object is colliding with this object.
        /// </summary>
        /// <param name="o">An object that could be colliding with this object.</param>
        bool IsCollidingWith(IGameObject o);

        /// <summary>
        /// Called when the object is ontop of another object or when the object is inside terrain.
        /// </summary>
        void OnCollision(IGameObject collider);

        /// <summary>
        /// Sets the object's team.
        /// </summary>
        /// <param name="team">TeamId.BLUE/PURPLE/NEUTRAL</param>
        void SetTeam(TeamId team);

        /// <summary>
        /// Returns the units that the game object travels each second. Default 0 unless overriden.
        /// </summary>
        float GetMoveSpeed();
		
		/// <summary>
		/// Returns the vector which represents the 2d orientation that the object is moving in.
		/// </summary>
        Vector2 GetDirection();

        /// <summary>
        /// Sets the object's path to the newWaypoints
        /// </summary>
        /// <param name="newWaypoints">New path of Vector2 coordinates that the unit will move to.</param>
        void SetWaypoints(List<Vector2> newWaypoints);

        /// <summary>
        /// Returns the waypoint that the object is currently moving to.
        /// </summary>
        Vector2 GetNextWaypoint();

        /// <summary>
        /// Whether or not the object has changed its waypoints since the last game update.
        /// </summary>
        bool IsMovementUpdated();

        /// <summary>
        /// Called every update after the object sets its waypoints.
        /// </summary>
        void ClearMovementUpdated();

        /// <summary>
        /// Whether or not the object has reached its final waypoint.
        /// </summary>
        bool IsPathEnded();

        /// <summary>
        /// Whether or not the object is networked to a specified team.
        /// </summary>
        /// <param name="team">A team which could have vision of this object.</param>
        bool IsVisibleByTeam(TeamId team);

        /// <summary>
        /// Sets the object to be networked or not to a specified team.
        /// </summary>
        /// <param name="team">A team which could have vision of this object.</param>
        /// <param name="visible">true/false; networked or not</param>
        void SetVisibleByTeam(TeamId team, bool visible);
    }
}
