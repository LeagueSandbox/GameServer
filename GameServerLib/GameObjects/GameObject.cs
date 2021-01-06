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
    public class GameObject : Target, IGameObject
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
        public override bool IsSimpleTarget => false;

        /// <summary>
        ///  Identifier unique to this game object.
        /// </summary>
        public uint NetId { get; }
        /// <summary>
        /// Radius of the circle which is used for collision detection between objects or terrain.
        /// </summary>
        public float CollisionRadius { get; set; }
        /// <summary>
        /// Waypoints that make up the path a game object is walking in.
        /// </summary>
        /// TODO: Move this to AttackableUnit, as GameObjects should be able to move or target.
        public List<Vector2> Waypoints { get; private set; }
        /// <summary>
        /// Index of the waypoint in the list of waypoints that the object is currently on.
        /// </summary>
        /// TODO: Move this to AttackableUnit, as GameObjects should be able to move or target.
        public int WaypointIndex { get; private set; }
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
        /// Current target the game object is looking at, moving to, or attacking (can be coordinates or an object)
        /// </summary>
        /// TODO: Remove the Target class and replace with IAttackableUnit.
        /// TODO: GameObjects and AttackableUnits shouldn't be able to target, so move this to ObjAIBase.
        public ITarget Target { get; set; }

        /// <summary>
        /// Instantiation of an object which represents the base class for all objects in League of Legends.
        /// </summary>
        public GameObject(Game game, float x, float y, int collisionRadius = 40, int visionRadius = 0, uint netId = 0, TeamId team = TeamId.TEAM_NEUTRAL) : base(x, y)
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
            Target = null;
            SyncId = (uint)Environment.TickCount; // TODO: use movement manager to generate this
            CollisionRadius = collisionRadius;
            VisionRadius = visionRadius;
            Waypoints = new List<Vector2>();

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
            Move(diff);
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
            X = vec.X;
            Y = vec.Y;
        }

        /// <summary>
        /// Refers to the height that the object is at in 3D space.
        /// </summary>
        public virtual float GetHeight()
        {
            return _game.Map.NavigationGrid.GetHeightAtLocation(X, Y);
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
            return Vector2.DistanceSquared(new Vector2(X, Y), o.GetPosition()) < (CollisionRadius + o.CollisionRadius) * (CollisionRadius + o.CollisionRadius);
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
                Vector2 exit = _game.Map.NavigationGrid.GetClosestTerrainExit(GetPosition(), CollisionRadius + 1.0f);
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
        /// Returns the units that the game object travels each second. Default 0 unless overriden.
        /// </summary>
        /// TODO: Remove this as GameObject shouldn't have anything stat or movement related and make AttackableUnit.GetMoveSpeed virtual.
        public virtual float GetMoveSpeed()
        {
            return 0;
        }

        /// <summary>
        /// Moves the object to its specified waypoints, updating its coordinate.
        /// </summary>
        /// <param name="diff">The amount of milliseconds the object is supposed to move</param>
        /// TODO: Move this to AttackableUnit, as GameObjects should be able to move or target.
        public virtual void Move(float diff)
        {
            // no waypoints remained - clear the Waypoints
            if (WaypointIndex >= Waypoints.Count)
            {
                Waypoints.RemoveAll(v => v != Waypoints[0]);
                Target = null;
            }

            // TODO: Remove dependency of Target. In fact, just remove Target altogether.
            if (Target == null)
            {
                _direction = new Vector2();
                return;
            }

            // current -> next positions
            var cur = new Vector2(X, Y);
            var next = new Vector2(Target.X, Target.Y);

            if (cur == next)
            {
                return;
            }

            var goingTo = next - cur;
            _direction = Vector2.Normalize(goingTo);

            // usually doesn't happen
            if (float.IsNaN(_direction.X) || float.IsNaN(_direction.Y))
            {
                _direction = new Vector2(0, 0);
            }

            var moveSpeed = GetMoveSpeed();

            var dist = MathF.Abs(Vector2.Distance(cur, next));

            var deltaMovement = moveSpeed * 0.001f * diff;

            // Prevent moving past the next waypoint.
            if (deltaMovement > dist)
            {
                deltaMovement = dist;
            }

            var xx = _direction.X * deltaMovement;
            var yy = _direction.Y * deltaMovement;

            // TODO: Prevent movement past obstacles (after moving movement functionality to ObjAIBase).
            //Vector2 nextPos = new Vector2(X + xx, Y + yy);
            //KeyValuePair<bool, Vector2> pathBlocked = _game.Map.NavigationGrid.IsAnythingBetween(GetPosition(), nextPos);
            //if (pathBlocked.Key)
            //{
            //    nextPos = _game.Map.NavigationGrid.GetClosestTerrainExit(pathBlocked.Value, CollisionRadius + 1.0f);
            //}

            X += xx;
            Y += yy;

            // (X, Y) have now moved to the next position
            cur = new Vector2(X, Y);

            // Check if we reached the next waypoint
            // REVIEW (of previous code): (deltaMovement * 2) being used here is problematic; if the server lags, the diff will be much greater than the usual values
            if ((cur - next).LengthSquared() < MOVEMENT_EPSILON * MOVEMENT_EPSILON)
            {
                if (this is IProjectile && !Target.IsSimpleTarget)
                {
                    return;
                }

                // remove this waypoint because we have reached it
                if (++WaypointIndex >= Waypoints.Count)
                {
                    Target = null;
                }
                else
                {
                    Target = new Target(Waypoints[WaypointIndex]);
                }
            }
        }

        /// <summary>
        /// Returns the next waypoint. If all waypoints have been reached then this returns a -inf Vector2
        /// </summary>
        /// TODO: Move this to AttackableUnit, as GameObjects should be able to move or target.
        public Vector2 GetNextWaypoint()
        {
            if (WaypointIndex < Waypoints.Count)
            {
                return Waypoints[WaypointIndex];
            }
            return new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        }

        /// <summary>
        /// Returns whether the game object has reached the last waypoint in its path of waypoints.
        /// </summary>
        /// TODO: Move this to AttackableUnit, as GameObjects should be able to move or target.
        public bool IsPathEnded()
        {
            return WaypointIndex >= Waypoints.Count;
        }

        /// <summary>
        /// Sets the object's path to the newWaypoints
        /// </summary>
        /// <param name="newWaypoints">New path of Vector2 coordinates that the unit will move to.</param>
        /// TODO: Move this to AttackableUnit, as GameObjects should be able to move or target.
        public void SetWaypoints(List<Vector2> newWaypoints)
        {
            Waypoints = newWaypoints;

            _movementUpdated = true;
            if (Waypoints.Count == 1)
            {
                Target = null;
                return;
            }

            Target = new Target(Waypoints[1]);
            WaypointIndex = 1;
        }

        /// <summary>
        /// Returns whether the game object has set its waypoints this update.
        /// </summary>
        /// <returns></returns>
        /// TODO: Move this to AttackableUnit, as GameObjects should be able to move or target.
        public bool IsMovementUpdated()
        {
            return _movementUpdated;
        }

        /// <summary>
        /// Used each object manager update after the game object has set its waypoints and the server has networked it.
        /// </summary>
        /// <returns></returns>
        /// TODO: Move this to AttackableUnit, as GameObjects should be able to move or target.
        public void ClearMovementUpdated()
        {
            _movementUpdated = false;
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
        public void TeleportTo(float x, float y)
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
