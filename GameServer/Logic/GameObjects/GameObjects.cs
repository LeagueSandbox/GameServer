using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic
{
    public class GameObject : Target
    {
        protected uint id;
        protected float xvector, yvector;

        /**
         * Current target the object running to (can be coordinates or an object)
         */
        protected Target target;

        protected List<Vector2> waypoints = new List<Vector2>();
        protected int curWaypoint;
        protected Game _game;
        protected TeamId team;
        protected bool movementUpdated;
        protected bool toRemove;
        protected int attackerCount;
        protected int collisionRadius;
        protected Vector2 direction;
        protected int visionRadius;
        protected bool dashing;
        protected float dashSpeed;
        protected Dictionary<TeamId, bool> visibleByTeam;

        public GameObject(Game game, uint id, float x, float y, int collisionRadius, int visionRadius = 0) : base(x, y)
        {
            _game = game;
            this.id = id;
            this.target = null;
            this.collisionRadius = collisionRadius;
            this.visionRadius = visionRadius;
            this.team = 0;
            this.movementUpdated = false;
            this.toRemove = false;
            this.attackerCount = 0;
            this.dashing = false;
            this.visibleByTeam = new Dictionary<TeamId, bool>();

            var teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>();
            foreach (var team in teams)
                visibleByTeam.Add(team, false);
        }

        public virtual void onCollision(GameObject collider) { }

        /**
        * Moves the object depending on its target, updating its coordinate.
        * @param diff the amount of milliseconds the object is supposed to move
        */
        public void Move(long diff)
        {
            if (target == null)
            {
                direction = new Vector2();
                return;
            }
            var to = new Vector2(target.getX(), target.getY());
            var cur = new Vector2(getX(), getY()); //?


            var goingTo = (to - cur);
            direction = Vector2.Normalize(goingTo);
            if (float.IsNaN(direction.X) || float.IsNaN(direction.Y))
            {
                direction = new Vector2(0, 0);
            }

            float moveSpeed = dashing ? dashSpeed : getMoveSpeed();
            float deltaMovement = (moveSpeed) * 0.001f * diff;

            float xx = direction.X * deltaMovement;
            float yy = direction.Y * deltaMovement;

            x += xx;
            y += yy;

            /* If the target was a simple point, stop when it is reached */
            if (target.isSimpleTarget() && distanceWith(target) < deltaMovement * 2) //how can target be null here?????
            {
                if (dashing)
                {
                    dashing = false;
                    setTarget(null);
                }
                else if (++curWaypoint >= waypoints.Count)
                {
                    setTarget(null);
                }
                else
                {
                    setTarget(new Target(waypoints[curWaypoint]));
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

        /**
        * Sets the team of the object
        * @param team the new team
        */
        public void setTeam(TeamId team)
        {
            this.team = team;
            visibleByTeam[team] = true;
        }
        public TeamId getTeam()
        {
            return team;
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

        public Target getTarget()
        {
            return target;
        }
        public void setTarget(Target target)
        {
            if (this.target == target)
                return;

            this.target = target;
        }

        public void setWaypoints(List<Vector2> newWaypoints)
        {
            waypoints = newWaypoints;

            setPosition(waypoints[0].X, waypoints[0].Y);
            movementUpdated = true;
            if (waypoints.Count == 1)
            {
                setTarget(null);
                return;
            }

            setTarget(new Target(waypoints[1]));
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

        public uint getNetId()
        {
            return id;
        }

        public Game GetGame()
        {
            return _game;
        }

        public override void setPosition(float x, float y)
        {
            this.x = x;
            this.y = y;

            setTarget(null);
        }
        public virtual float GetZ()
        {
            return _game.GetMap().GetHeightAtLocation(x, y);
        }

        public void setCollisionRadius(int collisionRadius)
        {
            this.collisionRadius = collisionRadius;
        }

        public int getCollisionRadius()
        {
            return collisionRadius;
        }
        public virtual float getLargestRadius()
        {
            return collisionRadius;
        }

        public int getVisionRadius()
        {
            return visionRadius;
        }

        public bool collide(GameObject o)
        {
            return distanceWithSqr(o) < (getCollisionRadius() + o.getCollisionRadius()) * (getCollisionRadius() + o.getCollisionRadius());
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
            return (team == getTeam() || visibleByTeam[team]);
        }

        public void setVisibleByTeam(TeamId team, bool visible)
        {
            visibleByTeam[team] = visible;
        }

        public void dashTo(float x, float y, float dashSpeed)
        {
            dashing = true;
            this.dashSpeed = dashSpeed;
            setTarget(new Target(x, y));
            waypoints.Clear();
        }
        public bool isDashing()
        {
            return dashing;
        }
    }
}
