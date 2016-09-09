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

        protected List<Vector2> waypoints = new List<Vector2>();
        protected int curWaypoint;
        private TeamId _team;
        public TeamId Team
        {
            get { return _team; }
            set
            {
                this._team = value;
                visibleByTeam[_team] = true;
            }
        }
        protected bool movementUpdated;
        protected bool toRemove;
        protected int attackerCount;
        public int CollisionRadius { get; set; }
        protected Vector2 direction;
        public int VisionRadius { get; private set; }
        public bool IsDashing { get; private set; }
        private float _dashSpeed;
        protected Dictionary<TeamId, bool> visibleByTeam;
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

            this.visibleByTeam = new Dictionary<TeamId, bool>();
            var teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>();
            foreach (var team in teams)
            {
                visibleByTeam.Add(team, false);
            }

            this.Team = TeamId.TEAM_NEUTRAL;
            this.movementUpdated = false;
            this.toRemove = false;
            this.attackerCount = 0;
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
                direction = new Vector2();
                return;
            }
            var to = new Vector2(Target.getX(), Target.getY());
            var cur = new Vector2(getX(), getY()); //?


            var goingTo = (to - cur);
            direction = Vector2.Normalize(goingTo);
            if (float.IsNaN(direction.X) || float.IsNaN(direction.Y))
            {
                direction = new Vector2(0, 0);
            }

            float moveSpeed = getMoveSpeed();
            if (IsDashing)
            {
                moveSpeed = _dashSpeed;
            }

            float deltaMovement = (moveSpeed) * 0.001f * diff;

            float xx = direction.X * deltaMovement;
            float yy = direction.Y * deltaMovement;

            x += xx;
            y += yy;

            /* If the target was a simple point, stop when it is reached */
            if (Target.isSimpleTarget() && distanceWith(Target) < deltaMovement * 2) //how can target be null here?????
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
                else if (++curWaypoint >= waypoints.Count)
                {
                    Target = null;
                }
                else
                {
                    Target = new Target(waypoints[curWaypoint]);
                }
            }
        }
        public Vector2 getDirection()
        {
            return direction;
        }

        public void calculateVector(float xtarget, float ytarget)
        {
            xvector = xtarget - getX();
            yvector = ytarget - getY();

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

        public void setWaypoints(List<Vector2> newWaypoints)
        {
            waypoints = newWaypoints;

            setPosition(waypoints[0].X, waypoints[0].Y);
            movementUpdated = true;
            if (waypoints.Count == 1)
            {
                Target = null;
                return;
            }

            Target = new Target(waypoints[1]);
            curWaypoint = 1;
        }

        public List<Vector2> getWaypoints()
        {
            return waypoints;
        }

        public int getCurWaypoint()
        {
            return curWaypoint;
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
            this.x = x;
            this.y = y;

            Target = null;
        }

        public virtual float GetZ()
        {
            return _game.GetMap().GetHeightAtLocation(x, y);
        }

        public bool collide(GameObject o)
        {
            return distanceWithSqr(o) < (CollisionRadius + o.CollisionRadius) * (CollisionRadius + o.CollisionRadius);
        }

        public int getAttackerCount()
        {
            return attackerCount;
        }
        public void incrementAttackerCount()
        {
            ++attackerCount;
        }
        public void decrementAttackerCount()
        {
            --attackerCount;
        }

        public bool isVisibleByTeam(TeamId team)
        {
            return (team == Team || visibleByTeam[team]);
        }

        public void setVisibleByTeam(TeamId team, bool visible)
        {
            visibleByTeam[team] = visible;
        }

        public void dashTo(float x, float y, float dashSpeed)
        {
            IsDashing = true;
            this._dashSpeed = dashSpeed;
            Target = new Target(x, y);
            waypoints.Clear();
        }
    }
}
