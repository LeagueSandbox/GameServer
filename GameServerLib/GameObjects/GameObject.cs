using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.Missiles;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.Packets;

namespace LeagueSandbox.GameServer.GameObjects
{
    /// <summmary>
    /// Base class for all objects.
    /// GameObjects normally follow these guidelines of functionality: Position, Direction, Collision, Vision, Team, and Networking.
    /// </summmary>
    public class GameObject : IGameObject
    {
        // Crucial Vars (keep in mind Game is everywhere, which could be an issue for the future)
        protected Game _game;
        protected NetworkIdManager _networkIdManager;

        // Function Vars
        protected bool _toRemove;
        protected bool _movementUpdated;
        protected Vector2 _direction;
        private Dictionary<TeamId, bool> _visibleByTeam;

        /// <summary>
        /// Comparison variable for small distance movements.
        /// </summary>
        public static readonly uint MOVEMENT_EPSILON = 5; //TODO: Verify if this should be changed
        /// <summary>
        /// Whether or not this object counts as a single point target. *NOTE*: Will be depricated once Target class is removed.
        /// </summary>

        /// <summary>
        ///  Identifier unique to this game object.
        /// </summary>
        public uint NetId { get; }
        /// <summary>
        /// Radius of the circle which is used for collision detection between objects or terrain.
        /// </summary>
        public float CollisionRadius { get; set; }
        /// <summary>
        /// Position of this GameObject from a top-down view.
        /// </summary>
        public Vector2 Position { get; protected set; }
        /// <summary>
        /// Used to synchronize movement between client and server. Is currently assigned Env.TickCount.
        /// </summary>
        public uint SyncId { get; }
        /// <summary>
        /// Team identifier, refer to TeamId enum.
        /// </summary>
        public TeamId Team { get; protected set; }
        /// <summary>
        /// Radius of the circle which is used for vision; detecting if objects are visible given terrain, and if so, networked to the player (or team) that owns this game object.
        /// </summary>
        public float VisionRadius { get; protected set; }

        /// <summary>
        /// Instantiation of an object which represents the base class for all objects in League of Legends.
        /// </summary>
        public GameObject(Game game, Vector2 position, int collisionRadius = 40, int visionRadius = 0, uint netId = 0, TeamId team = TeamId.TEAM_NEUTRAL)
        {
            _game = game;
            _networkIdManager = game.NetworkIdManager;
            if (netId != 0)
            {
                NetId = netId; // Custom netId
            }
            else
            {
                NetId = _networkIdManager.GetNewNetId(); // base class assigns a netId
            }
            Position = position;
            SyncId = (uint)Environment.TickCount; // TODO: use movement manager to generate this
            CollisionRadius = collisionRadius;
            VisionRadius = visionRadius;

            _visibleByTeam = new Dictionary<TeamId, bool>();
            var teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>();
            foreach (var t in teams)
            {
                _visibleByTeam.Add(t, false);
            }

            Team = team;
            _movementUpdated = false;
            _toRemove = false;
        }

        /// <summary>
        /// Called by ObjectManager after AddObject (usually right after instatiation of GameObject).
        /// </summary>
        public virtual void OnAdded()
        {
            _game.Map.CollisionHandler.AddObject(this);
        }

        /// <summary>
        /// Called by ObjectManager every tick.
        /// </summary>
        /// <param name="diff">Number of milliseconds that passed before this tick occurred.</param>
        public virtual void Update(float diff)
        {
        }

        /// <summary>
        /// Whether or not the object should be removed from the game (usually both server and client-side). Refer to ObjectManager.
        /// </summary>
        public bool IsToRemove()
        {
            return _toRemove;
        }

        /// <summary>
        /// Will cause ObjectManager to remove the object (usually) both server and client-side next update.
        /// </summary>
        public virtual void SetToRemove()
        {
            _toRemove = true;
        }

        /// <summary>
        /// Called by ObjectManager after the object has been SetToRemove.
        /// </summary>
        public virtual void OnRemoved()
        {
            _game.Map.CollisionHandler.RemoveObject(this);
        }

        /// <summary>
        /// Refers to the height that the object is at in 3D space.
        /// </summary>
        public virtual float GetHeight()
        {
            return _game.Map.NavigationGrid.GetHeightAtLocation(Position.X, Position.Y);
        }

        /// <summary>
        /// Gets the position of this GameObject in 3D space, where the Y value represents height.
        /// Mostly used for packets.
        /// </summary>
        /// <returns>Vector3 position.</returns>
        public Vector3 GetPosition3D()
        {
            return new Vector3(Position.X, GetHeight(), Position.Y);
        }

        /// <summary>
        /// Sets the server-sided position of this object.
        /// </summary>
        public virtual void SetPosition(float x, float y)
        {
            SetPosition(new Vector2(x, y));
        }

        /// <summary>
        /// Sets the server-sided position of this object.
        /// </summary>
        public virtual void SetPosition(Vector2 vec)
        {
            Position = vec;
        }

        /// <summary>
        /// Returns the current direction (from 2D top-down perspective) used in movement.
        /// </summary>
        public Vector2 GetDirection()
        {
            return _direction;
        }

        /// <summary>
        /// Whether or not the specified object is colliding with this object.
        /// </summary>
        /// <param name="o">An object that could be colliding with this object.</param>
        public virtual bool IsCollidingWith(IGameObject o)
        {
            return Vector2.DistanceSquared(new Vector2(Position.X, Position.Y), o.Position) < (CollisionRadius + o.CollisionRadius) * (CollisionRadius + o.CollisionRadius);
        }

        /// <summary>
        /// Called by ObjectManager when the object is ontop of another object or when the object is inside terrain.
        /// </summary>
        public virtual void OnCollision(IGameObject collider, bool isTerrain = false)
        {
            if (isTerrain)
            {
                // Escape functionality should be moved to GameObject.OnCollision.
                // only time we would collide with terrain is if we are inside of it, so we should teleport out of it.
                Vector2 exit = _game.Map.NavigationGrid.GetClosestTerrainExit(Position, CollisionRadius + 1.0f);
                TeleportTo(exit.X, exit.Y);
            }
        }

        /// <summary>
        /// Sets the object's team.
        /// </summary>
        /// <param name="team">TeamId.BLUE/PURPLE/NEUTRAL</param>
        public void SetTeam(TeamId team)
        {
            _visibleByTeam[Team] = false;
            Team = team;
            _visibleByTeam[Team] = true;
            if (_game.IsRunning)
            {
                _game.PacketNotifier.NotifySetTeam(this as IAttackableUnit);
            }
        }

        /// <summary>
        /// Whether or not the object is networked to a specified team.
        /// </summary>
        /// <param name="team">A team which could have vision of this object.</param>
        public bool IsVisibleByTeam(TeamId team)
        {
            return team == Team || _visibleByTeam[team];
        }

        /// <summary>
        /// Sets the object to be networked or not to a specified team.
        /// </summary>
        /// <param name="team">A team which could have vision of this object.</param>
        /// <param name="visible">true/false; networked or not</param>
        public void SetVisibleByTeam(TeamId team, bool visible)
        {
            _visibleByTeam[team] = visible;

            if (this is IAttackableUnit)
            {
                // TODO: send this in one place only
                _game.PacketNotifier.NotifyUpdatedStats(this as IAttackableUnit, false);
            }
        }

        /// <summary>
        /// Sets the position of this GameObject to the specified position.
        /// </summary>
        /// <param name="x">X coordinate to set.</param>
        /// <param name="y">Y coordinate to set.</param>
        public virtual void TeleportTo(float x, float y)
        {
            if (!_game.Map.NavigationGrid.IsWalkable(x, y, CollisionRadius))
            {
                var walkableSpot = _game.Map.NavigationGrid.GetClosestTerrainExit(new Vector2(x, y), CollisionRadius + 1.0f);
                SetPosition(walkableSpot);

                x = MovementVector.TargetXToNormalFormat(_game.Map.NavigationGrid, walkableSpot.X);
                y = MovementVector.TargetYToNormalFormat(_game.Map.NavigationGrid, walkableSpot.Y);
            }
            else
            {
                SetPosition(x, y);

                x = MovementVector.TargetXToNormalFormat(_game.Map.NavigationGrid, x);
                y = MovementVector.TargetYToNormalFormat(_game.Map.NavigationGrid, y);
            }

            _game.PacketNotifier.NotifyTeleport(this, new Vector2(x, y));
        }
    }
}
