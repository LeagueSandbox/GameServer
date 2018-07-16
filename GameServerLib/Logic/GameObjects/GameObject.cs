using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.Missiles;
using LeagueSandbox.GameServer.Logic.GameObjects.Other;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class GameObject : Target
    {
        public uint NetId { get; private set; }
        protected float _xvector, _yvector;

        /// <summary>
        /// Current target the object running to (can be coordinates or an object)
        /// </summary>
        public Target Target { get; set; }

        public List<Vector2> Waypoints { get; private set; }
        public int CurWaypoint { get; private set; }
        public TeamId Team { get; protected set; }

        public void SetTeam(TeamId team)
        {
            _visibleByTeam[Team] = false;
            Team = team;
            _visibleByTeam[Team] = true;
            if (_game.IsRunning)
            {
                var p = new SetTeam(_game, this as AttackableUnit, team);
                _game.PacketHandlerManager.BroadcastPacket(p, Channel.CHL_S2_C);
            }
        }

        protected bool _movementUpdated;
        protected bool _toRemove;
        public int AttackerCount { get; private set; }
        public float CollisionRadius { get; set; }
        protected Vector2 _direction;
        public float VisionRadius { get; protected set; }
        public bool IsDashing { get; protected set; }
        public override bool IsSimpleTarget => false;
        protected float _dashSpeed;
        private Dictionary<TeamId, bool> _visibleByTeam;
        protected Game _game;
        protected NetworkIdManager _networkIdManager;

        public GameObject(Game game, float x, float y, int collisionRadius, int visionRadius = 0, uint netId = 0) : base(x, y)
        {
            _game = game;
            _networkIdManager = game.GetNetworkManager();
            if (netId != 0)
            {
                NetId = netId; // Custom netId
            }
            else
            {
                NetId = _networkIdManager.GetNewNetId(); // Let the base class (this one) asign a netId
            }
            Target = null;
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
            AttackerCount = 0;
            IsDashing = false;
        }

        public virtual void OnAdded()
        {
            _game.Map.CollisionHandler.AddObject(this);
        }

        public virtual void OnRemoved()
        {
            _game.Map.CollisionHandler.RemoveObject(this);
        }

        public virtual void OnCollision(GameObject collider)
        {
        }

        /// <summary>
        /// Moves the object depending on its target, updating its coordinate.
        /// </summary>
        /// <param name="diff">The amount of milliseconds the object is supposed to move</param>
        public void Move(float diff)
        {
            if (Target == null)
            {
                _direction = new Vector2();
                return;
            }

            var to = new Vector2(Target.X, Target.Y);
            var cur = new Vector2(X, Y); //?

            var goingTo = to - cur;
            _direction = Vector2.Normalize(goingTo);
            if (float.IsNaN(_direction.X) || float.IsNaN(_direction.Y))
            {
                _direction = new Vector2(0, 0);
            }

            var moveSpeed = GetMoveSpeed();
            if (IsDashing)
            {
                moveSpeed = _dashSpeed;
            }

            var deltaMovement = moveSpeed * 0.001f * diff;

            var xx = _direction.X * deltaMovement;
            var yy = _direction.Y * deltaMovement;

            X += xx;
            Y += yy;

            // If the target was a simple point, stop when it is reached

            if (GetDistanceTo(Target) < deltaMovement * 2)
            {
                if (this is Projectile && !Target.IsSimpleTarget)
                {
                    return;
                }

                if (IsDashing)
                {
                    if (this is AttackableUnit)
                    {
                        var u = this as AttackableUnit;

                        var animList = new List<string>();
                        _game.PacketNotifier.NotifySetAnimation(u, animList);
                    }

                    Target = null;
                }
                else if (++CurWaypoint >= Waypoints.Count)
                {
                    Target = null;
                }
                else
                {
                    Target = new Target(Waypoints[CurWaypoint]);
                }

                if (IsDashing)
                {
                    IsDashing = false;
                }
            }
        }

        public void CalculateVector(float xtarget, float ytarget)
        {
            _xvector = xtarget - X;
            _yvector = ytarget - Y;

            if (_xvector == 0 && _yvector == 0)
            {
                return;
            }

            var toDivide = Math.Abs(_xvector) + Math.Abs(_yvector);
            _xvector /= toDivide;
            _yvector /= toDivide;
        }

        public virtual void Update(float diff)
        {
            Move(diff);
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
            CurWaypoint = 1;
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
            return _game.Map.NavGrid.GetHeightAtLocation(X, Y);
        }

        public bool IsCollidingWith(GameObject o)
        {
            return GetDistanceToSqr(o) < (CollisionRadius + o.CollisionRadius) * (CollisionRadius + o.CollisionRadius);
        }

        public void IncrementAttackerCount()
        {
            ++AttackerCount;
        }
        public void DecrementAttackerCount()
        {
            --AttackerCount;
        }

        public bool IsVisibleByTeam(TeamId team)
        {
            return team == Team || _visibleByTeam[team];
        }

        public void SetVisibleByTeam(TeamId team, bool visible)
        {
            _visibleByTeam[team] = visible;

            if (this is AttackableUnit)
            {
                // TODO: send this in one place only
                _game.PacketNotifier.NotifyUpdatedStats(this as AttackableUnit, false);
            }
        }

        public void DashToTarget(Target t, float dashSpeed, float followTargetMaxDistance, float backDistance, float travelTime)
        {
            // TODO: Take into account the rest of the arguments
            IsDashing = true;
            _dashSpeed = dashSpeed;
            Target = t;
            Waypoints.Clear();
        }

        public void SetDashingState(bool state)
        {
            IsDashing = state;
        }
    }
}
