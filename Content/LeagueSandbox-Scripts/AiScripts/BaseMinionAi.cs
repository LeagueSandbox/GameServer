using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using System.Linq;

namespace AiScripts
{
    public class BaseMinonAi : IAiScript
    {
        public IAiScriptMetaData AiScriptMetaData { get; set; } = new AiScriptMetaData();
        IMinion minion;
        internal const float DETECT_RANGE = 475.0f;
        internal const int EXP_RANGE = 1400;
        public void OnActivate(IObjAiBase owner)
        {
            minion = owner as IMinion;
        }
        public void OnUpdate(float diff)
        {
            if (!minion.IsDead)
            {
                if (minion.MovementParameters != null || minion.IsAiPaused())
                {
                    return;
                }

                AIMove();
            }
        }

        protected bool ScanForTargets()
        {
            if (minion.TargetUnit != null && !minion.TargetUnit.IsDead)
            {
                return true;
            }

            IAttackableUnit nextTarget = null;
            var nextTargetPriority = 14;
            var nearestObjects = GetUnitsInRange(minion.Position, minion.Stats.Range.Total, true);
            //Find target closest to max attack range.
            foreach (var it in nearestObjects.OrderBy(x => Vector2.DistanceSquared(minion.Position, x.Position) - (minion.Stats.Range.Total * minion.Stats.Range.Total)))
            {
                if (!(it is IAttackableUnit u)
                    || u.IsDead
                    || u.Team == minion.Team
                    || Vector2.DistanceSquared(minion.Position, u.Position) > DETECT_RANGE * DETECT_RANGE
                    || !TeamHasVision(minion.Team, u)
                    || !u.Status.HasFlag(StatusFlags.Targetable)
                    || UnitIsProtected(u))
                {
                    continue;
                }

                var priority = (int)minion.ClassifyTarget(u);// get the priority.
                if (priority < nextTargetPriority) // if the priority is lower than the target we checked previously
                {
                    nextTarget = u;                // make it a potential target.
                    nextTargetPriority = priority;
                }
            }

            if (nextTarget != null) // If we have a target
            {
                // Set the new target and refresh waypoints
                minion.SetTargetUnit(nextTarget, true);

                return true;
            }

            return false;
        }
        public virtual bool AIMove()
        {
            if (ScanForTargets()) // returns true if we have a target
            {
                if (!minion.RecalculateAttackPosition())
                {
                    KeepFocusingTarget(); // attack/follow target
                }
                return false;
            }
            return true;
        }
        protected void KeepFocusingTarget()
        {
            if (minion.IsAttacking && (minion.TargetUnit == null || minion.TargetUnit.IsDead || Vector2.DistanceSquared(minion.Position, minion.TargetUnit.Position) > minion.Stats.Range.Total * minion.Stats.Range.Total))
            // If target is dead or out of range
            {
                minion.InstantStopAttack();
            }
        }
    }
}