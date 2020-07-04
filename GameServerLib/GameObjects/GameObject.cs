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
    public class GameObject : Target, IGameObject
    {
        protected bool _movementUpdated;
        protected bool _toRemove;
        public uint NetId { get; }
        public uint SyncId { get; }

        private static uint MOVEMENT_EPSILON = 5; //TODO: Verify if this should exist

        public int WaypointIndex { get; private set; }

        public List<Vector2> Waypoints { get; private set; }

        public TeamId Team { get; protected set; }
        public void SetTeam(TeamId team)
        {
            _visibleByTeam[Team] = false;
            Team = team;
            _visibleByTeam[Team] = true;
            if (_game.IsRunning)
            {
                _game.PacketNotifier.NotifySetTeam(this as IAttackableUnit, team);
            }
        }

        public float CollisionRadius { get; set; }
        public float VisionRadius { get; protected set; }
        public override bool IsSimpleTarget => false;
        protected Vector2 _direction;
        private Dictionary<TeamId, bool> _visibleByTeam;
        protected Game _game;
        protected NetworkIdManager _networkIdManager;

        /// <summary>
        /// Current target the game object is looking/attacking/moving to (can be coordinates or an object)
        /// </summary>
        public ITarget Target { get; set; }

        public GameObject(Game game, float x, float y, int collisionRadius, int visionRadius = 0, uint netId = 0) : base(x, y)
        {
            _game = game;
            _networkIdManager = game.NetworkIdManager;
            if (netId != 0)
            {
                NetId = netId; // Custom netId
            }
            else
            {
                NetId = _networkIdManager.GetNewNetId(); // Let the base class (this one) asign a netId
            }
            Target = null;
            SyncId = (uint)Environment.TickCount; // TODO: use movement manager to generate those
            CollisionRadius = collisionRadius;
            VisionRadius = visionRadius;
            Waypoints = new List<Vector2>();

            _visibleByTeam = new Dictionary<TeamId, bool>();
            var teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>();
            foreach (var team in teams)
            {
                _visibleByTeam.Add(team, false);
            }

            Team = TeamId.TEAM_NEUTRAL;
            _movementUpdated = false;
            _toRemove = false;
        }

        public virtual void OnAdded()
        {
            _game.Map.CollisionHandler.AddObject(this);
        }

        public virtual void OnRemoved()
        {
            _game.Map.CollisionHandler.RemoveObject(this);
        }

        public virtual void OnCollision(IGameObject collider)
        {
        }

        /// <summary>
        /// Moves the object to its specified waypoints, updating its coordinate.
        /// </summary>
        /// <param name="diff">The amount of milliseconds the object is supposed to move</param>
        public void Move(float diff, bool useTarget = false)
        {
            // no waypoints remained - clear the Waypoints
            if (WaypointIndex >= Waypoints.Count)
            {
                Waypoints.RemoveAll(v => v != Waypoints[0]);
            }

            // TODO: Remove dependency of Target. In fact, just remove Target altogether.
            if (Target == null)
            {
                _direction = new Vector2();

                return;
            }

            // current position
            var cur = new Vector2(X, Y);

            var next = new Vector2();
            if (!Target.IsSimpleTarget)
            {
                next = new Vector2(Target.X, Target.Y);
            }
            else
            {
                next = Waypoints[WaypointIndex];
            }

            var goingTo = next - cur;
            _direction = Vector2.Normalize(goingTo);

            // usually doesn't happen
            if (float.IsNaN(_direction.X) || float.IsNaN(_direction.Y))
            {
                _direction = new Vector2(0, 0);
            }

            var moveSpeed = GetMoveSpeed();

            var deltaMovement = moveSpeed * 0.001f * diff;

            var xx = _direction.X * deltaMovement;
            var yy = _direction.Y * deltaMovement;

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
        /// Returns the current direction used in movement.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetDirection()
        {
            return _direction;
        }

        /// <summary>
        /// Returns the next waypoint. If all waypoints have been reached then this returns a -inf Vector2
        /// </summary>
        /// <returns></returns>
        public Vector2 GetNextWaypoint()
        {
            if (WaypointIndex < Waypoints.Count) return Waypoints[WaypointIndex];
            return new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        }
        /// <summary>
        /// Returns whether the game object has reached the last waypoint in its path of waypoints.
        /// </summary>
        /// <returns></returns>
        public bool IsPathEnded()
        {
            return WaypointIndex >= Waypoints.Count;
        }

        public virtual void Update(float diff)
        {
            if (Target != null && !Target.IsSimpleTarget)
            {
                Move(diff, true);
            }
            else
            {
                Move(diff);
            }
        }

        public virtual float GetMoveSpeed()
        {
            return 0;
        }

        public void SetWaypoints(List<Vector2> newWaypoints)
        {
            Waypoints = newWaypoints;

            SetPosition(Waypoints[0].X, Waypoints[0].Y);
            _movementUpdated = true;
            if (Waypoints.Count == 1)
            {
                Target = null;
                return;
            }

            Target = new Target(Waypoints[1]);
            WaypointIndex = 1;
        }

        public bool IsMovementUpdated()
        {
            return _movementUpdated;
        }

        public void ClearMovementUpdated()
        {
            _movementUpdated = false;
        }

        public bool IsToRemove()
        {
            return _toRemove;
        }

        public virtual void SetToRemove()
        {
            _toRemove = true;
        }

        public virtual void SetPosition(float x, float y)
        {
            X = x;
            Y = y;

            Target = null;
        }

        public virtual void SetPosition(Vector2 vec)
        {
            X = vec.X;
            Y = vec.Y;
            Target = null;
        }

        public virtual float GetZ()
        {
            return _game.Map.NavigationGrid.GetHeightAtLocation(X, Y);
        }

        public bool IsCollidingWith(IGameObject o)
        {
            return GetDistanceToSqr(o) < (CollisionRadius + o.CollisionRadius) * (CollisionRadius + o.CollisionRadius);
        }

        public bool IsVisibleByTeam(TeamId team)
        {
            return team == Team || _visibleByTeam[team];
        }

        public void SetVisibleByTeam(TeamId team, bool visible)
        {
            _visibleByTeam[team] = visible;

            if (this is IAttackableUnit)
            {
                // TODO: send this in one place only
                _game.PacketNotifier.NotifyUpdatedStats(this as IAttackableUnit, false);
            }
        }
    }
}
