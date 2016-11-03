using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace LeagueSandbox.GameServer.Logic
{
    public class GameObject : Target
    {
        public uint NetId { get; private set; }
        protected float xvector, yvector;

        /**
         * Current target the object running to (can be coordinates or an object)
         */
        public Target Target { get; set; }

        public List<Vector2> Waypoints { get; private set; }
        public int CurWaypoint { get; private set; }
        public TeamId Team { get; private set; }
        public void SetTeam(TeamId team)
        {
            _visibleByTeam[Team] = false;
            Team = team;
            _visibleByTeam[Team] = true;
            if (_game.IsRunning)
            {
                var p = new SetTeam(this as Unit, team);
                _game.PacketHandlerManager.broadcastPacket(p, Core.Logic.PacketHandlers.Channel.CHL_S2C);
            }
        }

        protected bool movementUpdated;
        protected bool toRemove;
        public int AttackerCount { get; private set; }
        public int CollisionRadius { get; set; }
        protected Vector2 _direction;
        public int VisionRadius { get; private set; }
        public bool IsDashing { get; protected set; }
        public override bool IsSimpleTarget { get { return false; } }
        protected float _dashSpeed;
        private Dictionary<TeamId, bool> _visibleByTeam;
        protected Game _game = Program.ResolveDependency<Game>();
        protected NetworkIdManager _networkIdManager = Program.ResolveDependency<NetworkIdManager>();

        public GameObject(float x, float y, int collisionRadius, int visionRadius = 0, uint netId = 0) : base(x, y)
        {
            if (netId != 0)
            {
                NetId = netId; // Custom netId
            }
            else
            {
                NetId = _networkIdManager.GetNewNetID(); // Let the base class (this one) asign a netId
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
            movementUpdated = false;
            toRemove = false;
            AttackerCount = 0;
            IsDashing = false;
        }

        public virtual void onCollision(GameObject collider) { }

        /**
        * Moves the object depending on its target, updating its coordinate.
        * @param diff the amount of milliseconds the object is supposed to move
        */
        public void Move(long diff)
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

            var moveSpeed = getMoveSpeed();
            if (IsDashing)
            {
                moveSpeed = _dashSpeed;
            }

            var deltaMovement = (moveSpeed) * 0.001f * diff;

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
                    if (this is Unit)
                    {
                        var u = this as Unit;

                        var animList = new List<string>();
                        _game.PacketNotifier.notifySetAnimation(u, animList);
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
            xvector = xtarget - X;
            yvector = ytarget - Y;

            if (xvector == 0 && yvector == 0)
                return;

            float toDivide = Math.Abs(xvector) + Math.Abs(yvector);
            xvector /= toDivide;
            yvector /= toDivide;
        }

        public virtual void update(long diff)
        {
            Move(diff);
        }
        public virtual float getMoveSpeed()
        {
            return 0;
        }

        public void SetWaypoints(List<Vector2> newWaypoints)
        {
            Waypoints = newWaypoints;

            setPosition(Waypoints[0].X, Waypoints[0].Y);
            movementUpdated = true;
            if (Waypoints.Count == 1)
            {
                Target = null;
                return;
            }

            Target = new Target(Waypoints[1]);
            CurWaypoint = 1;
        }

        public bool isMovementUpdated()
        {
            return movementUpdated;
        }

        public void clearMovementUpdated()
        {
            movementUpdated = false;
        }

        public bool isToRemove()
        {
            return toRemove;
        }

        public virtual void setToRemove()
        {
            toRemove = true;
        }

        public virtual void setPosition(float x, float y)
        {
            X = x;
            Y = y;

            Target = null;
        }

        public virtual float GetZ()
        {
            return _game.Map.GetHeightAtLocation(X, Y);
        }

        public bool Collide(GameObject o)
        {
            return GetDistanceToSqr(o) < (CollisionRadius + o.CollisionRadius) * (CollisionRadius + o.CollisionRadius);
        }

        public void incrementAttackerCount()
        {
            ++AttackerCount;
        }
        public void decrementAttackerCount()
        {
            --AttackerCount;
        }

        public bool IsVisibleByTeam(TeamId team)
        {
            return (team == Team || _visibleByTeam[team]);
        }

        public void SetVisibleByTeam(TeamId team, bool visible)
        {
            _visibleByTeam[team] = visible;
        }

        public void DashToTarget(Target t, float dashSpeed, float followTargetMaxDistance, float backDistance, float travelTime)
        {
            // TODO: Take into account the rest of the arguments
            IsDashing = true;
            _dashSpeed = dashSpeed;
            Target = t;
            Waypoints.Clear();
        }
    }
}
