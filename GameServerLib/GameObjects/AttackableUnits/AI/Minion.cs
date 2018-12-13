using System.Linq;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class Minion : ObjAiBase, IMinion
    {
        public string Name { get; }
        public IObjAiBase Owner { get; } // We'll probably want to change this in the future
        protected bool _aiPaused;

        public Minion(
            IGame game,
            IObjAiBase owner,
            float x,
            float y,
            string model,
            string name,
            int visionRadius = 0,
            uint netId = 0
        ) : base((Game)game, model, new Stats.Stats(), 40, x, y, visionRadius, netId)
        {
            if (!(owner == null))
            {
                SetTeam(owner.Team);
            }

            Owner = owner;

            // Fix issues induced by having an empty model string
            CollisionRadius = _game.Config.ContentManager.GetCharData(Model).PathfindingCollisionRadius;

            SetVisibleByTeam(Team, true);

            MoveOrder = MoveOrder.MOVE_ORDER_MOVE;

            Name = name;
        }

        public void PauseAi(bool b)
        {
            _aiPaused = b;
        }

        public override void OnAdded()
        {
            base.OnAdded();
            if (!(Owner == null))
            {
                _game.PacketNotifier.NotifyMinionSpawned(this, Team);
            }
        }

        public override void Update(float diff)
        {
            base.Update(diff);
            if (!IsDead)
            {
                if (IsDashing || _aiPaused)
                {
                    return;
                }
                if (ScanForTargets()) // returns true if we have a target
                {
                    if (!RecalculateAttackPosition())
                    {
                        KeepFocusingTarget(); // attack/follow target
                    }
                }
            }
            Replication.Update();
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
            var objects = _game.ObjectManager.GetObjects();
            foreach (var it in objects.OrderBy(x => GetDistanceTo(x.Value) - Stats.Range.Total))//Find target closest to max attack range.
            {
                if (!(it.Value is IAttackableUnit u) ||
                    u.IsDead ||
                    u.Team == Team ||
                    GetDistanceTo(u) > DETECT_RANGE ||
                    !_game.ObjectManager.TeamHasVisionOn(Team, u))
                    continue;
                var priority = (int)ClassifyTarget(u);  // get the priority.
                if (priority < nextTargetPriority) // if the priority is lower than the target we checked previously
                {
                    nextTarget = u;                // make him a potential target.
                    nextTargetPriority = priority;
                }
            }
            if (nextTarget != null) // If we have a target
            {
                TargetUnit = nextTarget; // Set the new target and refresh waypoints
                _game.PacketNotifier.NotifySetTarget(this, nextTarget);
                return true;
            }
            _game.PacketNotifier.NotifyStopAutoAttack(this);
            IsAttacking = false;
            return false;
        }

        protected void KeepFocusingTarget()
        {
            if (IsAttacking && (TargetUnit == null || TargetUnit.IsDead || GetDistanceTo(TargetUnit) > Stats.Range.Total))
            // If target is dead or out of range
            {
                _game.PacketNotifier.NotifyStopAutoAttack(this);
                IsAttacking = false;
            }
        }
    }
}
