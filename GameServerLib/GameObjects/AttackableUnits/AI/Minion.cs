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
        public string Name { get; protected set; }
        public IObjAiBase Owner { get; } // We'll probably want to change this in the future
        public bool IsWard { get; protected set; }
        public bool IsPet { get; protected set; }
        public bool IsBot { get; protected set; }
        public bool IsLaneMinion { get; protected set; }
        public bool IsClone { get; protected set; }
        protected bool _aiPaused;

        public Minion(
            Game game,
            IObjAiBase owner,
            Vector2 position,
            string model,
            string name,
            uint netId = 0,
            TeamId team = TeamId.TEAM_NEUTRAL
        ) : base(game, model, new Stats.Stats(), 40, position, 1100, netId, team)
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

            SetVisibleByTeam(Team, true);

            MoveOrder = OrderType.MoveTo;

            Replication = new ReplicationMinion(this);
        }

        public void PauseAi(bool b)
        {
            _aiPaused = b;
        }

        public override void OnAdded()
        {
            base.OnAdded();
            _game.PacketNotifier.NotifySpawn(this);
        }

        public override void Update(float diff)
        {
            base.Update(diff);
            if (!IsDead)
            {
                if (MovementParameters != null || _aiPaused)
                {
                    Replication.Update();
                    return;
                }

                AIMove();
            }
            Replication.Update();
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
            if(TargetUnit != null && !TargetUnit.IsDead)
            {
                return true;
            }

            IAttackableUnit nextTarget = null;
            var nextTargetPriority = 14;
            var nearestObjects = _game.Map.CollisionHandler.QuadDynamic.GetNearestObjects(this);
            //Find target closest to max attack range.
            foreach (var it in nearestObjects.OrderBy(x => Vector2.DistanceSquared(Position, x.Position) - (Stats.Range.Total * Stats.Range.Total)))
            {
                if (!(it is IAttackableUnit u) ||
                    u.IsDead ||
                    u.Team == Team ||
                    Vector2.DistanceSquared(Position, u.Position) > DETECT_RANGE * DETECT_RANGE ||
                    !_game.ObjectManager.TeamHasVisionOn(Team, u))
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
