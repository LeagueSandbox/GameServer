using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic
{
    public class GameObject : Target
    {
        public uint NetId { get; private set; }
        protected float xvector, yvector;

        /// <summary>
        /// Current target the object running to (can be coordinates or an object)
        /// </summary>
        public Target Target { get; set; }
        public TeamId Team { get; protected set; }

        public void SetTeam(TeamId team)
        {
            _visibleByTeam[Team] = false;
            Team = team;
            _visibleByTeam[Team] = true;
            if (_game.IsRunning)
            {
                var p = new SetTeam(this as AttackableUnit, team);
                _game.PacketHandlerManager.broadcastPacket(p, Channel.CHL_S2C);
            }
        }

        protected bool toRemove;
        public int AttackerCount { get; private set; }
        public float CollisionRadius { get; set; }
        protected Vector2 _direction;
        public float VisionRadius { get; protected set; }
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

            _visibleByTeam = new Dictionary<TeamId, bool>();
            var teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>();
            foreach (var team in teams)
            {
                _visibleByTeam.Add(team, false);
            }

            Team = TeamId.TEAM_NEUTRAL;
            toRemove = false;
            AttackerCount = 0;
        }

        public virtual void OnAdded()
        {
            _game.Map.CollisionHandler.AddObject(this);
        }

        public virtual void OnRemoved()
        {
            _game.Map.CollisionHandler.RemoveObject(this);
        }

        public virtual void onCollision(GameObject collider) { }

        /// <summary>
        /// Moves the object depending on its target, updating its coordinate.
        /// </summary>
        /// <param name="diff">The amount of milliseconds the object is supposed to move</param>
        public virtual void Move(float diff)
        {

        }

        public void CalculateVector(float xtarget, float ytarget)
        {
            xvector = xtarget - X;
            yvector = ytarget - Y;

            if (xvector == 0 && yvector == 0)
                return;

            var toDivide = Math.Abs(xvector) + Math.Abs(yvector);
            xvector /= toDivide;
            yvector /= toDivide;
        }

        public virtual void update(float diff)
        {
            Move(diff);
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

        public virtual void setPosition(Vector2 vec)
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
            return team == Team || _visibleByTeam[team];
        }

        public void SetVisibleByTeam(TeamId team, bool visible)
        {
            _visibleByTeam[team] = visible;
            if (this is AttackableUnit)
            {
                _game.PacketNotifier.NotifyUpdatedStats(this as AttackableUnit, false);
            }
        }
    }
}
