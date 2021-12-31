using System.Linq;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.Stats;
using GameServerCore.Enums;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class Minion : ObjAiBase, IMinion
    {
        protected bool _aiPaused;

        /// <summary>
        /// Unit which spawned this minion.
        /// </summary>
        public IObjAiBase Owner { get; }
        /// <summary>
        /// Whether or not this minion is considered a clone of its owner.
        /// </summary>
        public bool IsClone { get; protected set; }
        /// <summary>
        /// Whether or not this minion should ignore collisions.
        /// </summary>
        public bool IgnoresCollision { get; protected set; }
        /// <summary>
        /// Whether or not this minion is considered a ward.
        /// </summary>
        public bool IsWard { get; protected set; }
        /// <summary>
        /// Whether or not this minion is a LaneMinion.
        /// </summary>
        public bool IsLaneMinion { get; protected set; }
        /// <summary>
        /// Whether or not this minion is considered a pet.
        /// </summary>
        public bool IsPet { get; protected set; }
        /// <summary>
        /// Whether or not this minion is targetable at all.
        /// </summary>
        public bool IsTargetable { get; protected set; }
        /// <summary>
        /// Internal name of the minion.
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// Only unit which is allowed to see this minion.
        /// </summary>
        public IObjAiBase VisibilityOwner { get; }

        public Minion(
            Game game,
            IObjAiBase owner,
            Vector2 position,
            string model,
            string name,
            uint netId = 0,
            TeamId team = TeamId.TEAM_NEUTRAL,
            int skinId = 0,
            bool ignoreCollision = false,
            bool targetable = true,
            IObjAiBase visibilityOwner = null
        ) : base(game, model, new Stats.Stats(), 40, position, 1100, skinId, netId, team)
        {
            Name = name;

            Owner = owner;

            IsPet = false;
            if (Owner != null)
            {
                IsPet = true;
                if (model == Owner.Model) // Placeholder, should be changed
                {
                    IsClone = true;
                }
            }

            IsLaneMinion = false;
            IgnoresCollision = ignoreCollision;
            if (IgnoresCollision)
            {
                SetStatus(StatusFlags.Ghosted, true);
            }

            IsTargetable = targetable;
            if (!IsTargetable)
            {
                SetStatus(StatusFlags.Targetable, false);
            }

            VisibilityOwner = visibilityOwner;

            MoveOrder = OrderType.Stop;

            Replication = new ReplicationMinion(this);
        }

        public void PauseAi(bool b)
        {
            _aiPaused = b;
        }

        public override void OnAdded()
        {
            base.OnAdded();
        }

        public override void Update(float diff)
        {
            base.Update(diff);
            if (!IsDead)
            {
                if (MovementParameters != null || _aiPaused)
                {
                    return;
                }

                AIMove();
            }
        }

        public virtual bool AIMove()
        {
            if (ScanForTargets()) // returns true if we have a target
            {
                if (!RecalculateAttackPosition())
                {
                    KeepFocusingTarget(); // attack/follow target
                }
                return false;
            }
            return true;
        }

        // AI tasks
        protected bool ScanForTargets()
        {
            if (TargetUnit != null && !TargetUnit.IsDead)
            {
                return true;
            }

            IAttackableUnit nextTarget = null;
            var nextTargetPriority = 14;
            var nearestObjects = _game.Map.CollisionHandler.QuadDynamic.GetNearestObjects(this);
            //Find target closest to max attack range.
            foreach (var it in nearestObjects.OrderBy(x => Vector2.DistanceSquared(Position, x.Position) - (Stats.Range.Total * Stats.Range.Total)))
            {
                if (!(it is IAttackableUnit u)
                    || u.IsDead
                    || u.Team == Team
                    || Vector2.DistanceSquared(Position, u.Position) > DETECT_RANGE * DETECT_RANGE
                    || !_game.ObjectManager.TeamHasVisionOn(Team, u)
                    || !u.Status.HasFlag(StatusFlags.Targetable)
                    || _game.ProtectionManager.IsProtected(u))
                {
                    continue;
                }

                var priority = (int)ClassifyTarget(u);  // get the priority.
                if (priority < nextTargetPriority) // if the priority is lower than the target we checked previously
                {
                    nextTarget = u;                // make it a potential target.
                    nextTargetPriority = priority;
                }
            }

            if (nextTarget != null) // If we have a target
            {
                // Set the new target and refresh waypoints
                SetTargetUnit(nextTarget, true);

                return true;
            }

            return false;
        }

        protected void KeepFocusingTarget()
        {
            if (IsAttacking && (TargetUnit == null || TargetUnit.IsDead || Vector2.DistanceSquared(Position, TargetUnit.Position) > Stats.Range.Total * Stats.Range.Total))
            // If target is dead or out of range
            {
                _game.PacketNotifier.NotifyNPC_InstantStop_Attack(this, false);
                IsAttacking = false;
            }
        }
    }
}