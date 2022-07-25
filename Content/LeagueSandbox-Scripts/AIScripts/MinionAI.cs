using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using System.Linq;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;

namespace AIScripts
{
    public class MinonAI : IAIScript
    {
        public AIScriptMetaData AIScriptMetaData { get; set; } = new AIScriptMetaData();
        Minion minion;
        internal const float DETECT_RANGE = 475.0f;
        public void OnActivate(ObjAIBase owner)
        {
            minion = owner as Minion;
        }
        public void OnUpdate(float diff)
        {
            if (!minion.IsDead)
            {
                if (minion.MovementParameters != null || minion.IsAIPaused())
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

            AttackableUnit nextTarget = null;
            var nextTargetPriority = 14;
            var nearestObjects = GetUnitsInRange(minion.Position, minion.Stats.Range.Total, true);
            //Find target closest to max attack range.
            foreach (var it in nearestObjects.OrderBy(x => Vector2.DistanceSquared(minion.Position, x.Position) - (minion.Stats.Range.Total * minion.Stats.Range.Total)))
            {
                if (!(it is AttackableUnit u)
                    || u.IsDead
                    || u.Team == minion.Team
                    || Vector2.DistanceSquared(minion.Position, u.Position) > DETECT_RANGE * DETECT_RANGE
                    || !u.IsVisibleByTeam(minion.Team)
                    || !u.Status.HasFlag(StatusFlags.Targetable)
                    || UnitIsProtectionActive(u))
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
                minion.CancelAutoAttack(false, true);
                minion.SetTargetUnit(null, true);
            }
        }
    }
}