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
        }

        protected bool movementUpdated;
        protected bool toRemove;
        public int AttackerCount { get; private set; }
        public int CollisionRadius { get; set; }
        private Vector2 _direction;
        public int VisionRadius { get; private set; }
        public bool IsDashing { get; private set; }
        private float _dashSpeed;
        private Dictionary<TeamId, bool> _visibleByTeam;
        protected Game _game = Program.ResolveDependency<Game>();
        protected NetworkIdManager _networkIdManager = Program.ResolveDependency<NetworkIdManager>();

        public GameObject(float x, float y, int collisionRadius, int visionRadius = 0, uint netId = 0) : base(x, y)
        {
            if (netId != 0)
            {
                this.NetId = netId; // Custom netId
            }
            else
            {
                this.NetId = _networkIdManager.GetNewNetID(); // Let the base class (this one) asign a netId
            }
            this.Target = null;
            this.CollisionRadius = collisionRadius;
            this.VisionRadius = visionRadius;
            Waypoints = new List<Vector2>();

            this._visibleByTeam = new Dictionary<TeamId, bool>();
            var teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>();
            foreach (var team in teams)
            {
                _visibleByTeam.Add(team, false);
            }

            this.Team = TeamId.TEAM_NEUTRAL;
            this.movementUpdated = false;
            this.toRemove = false;
            this.AttackerCount = 0;
            this.IsDashing = false;



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


            var goingTo = (to - cur);
            _direction = Vector2.Normalize(goingTo);
            if (float.IsNaN(_direction.X) || float.IsNaN(_direction.Y))
            {
                _direction = new Vector2(0, 0);
            }

            float moveSpeed = getMoveSpeed();
            if (IsDashing)
            {
                moveSpeed = _dashSpeed;
            }

            float deltaMovement = (moveSpeed) * 0.001f * diff;

            float xx = _direction.X * deltaMovement;
            float yy = _direction.Y * deltaMovement;

            X += xx;
            Y += yy;

            /* If the target was a simple point, stop when it is reached */
            if (Target.isSimpleTarget() && GetDistanceTo(Target) < deltaMovement * 2) //how can target be null here?????
            {
                if (IsDashing)
                {
                    IsDashing = false;

                    if (this is Unit)
                    {
                        var u = this as Unit;

                        List<string> animList = new List<string>();
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

        public override bool isSimpleTarget()
        {
            return false;
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

        public override void setPosition(float x, float y)
        {
            this.X = x;
            this.Y = y;

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

        public void DashTo(float x, float y, float dashSpeed)
        {
            IsDashing = true;
            this._dashSpeed = dashSpeed;
            Target = new Target(x, y);
            Waypoints.Clear();
        }
    }
}
