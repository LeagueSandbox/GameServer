using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using System.Linq;

namespace AIScripts
{
    public class TurretAI : IAIScript
    {
        public IAIScriptMetaData AIScriptMetaData { get; set; } = new AIScriptMetaData();
        IBaseTurret baseTurret;

        public void OnActivate(IObjAiBase owner)
        {
            baseTurret = owner as IBaseTurret;
        }
        public void OnUpdate(float diff)
        {
            if (!baseTurret.IsAttacking)
            {
                CheckForTargets();
            }

            // Lose focus of the unit target if the target is out of range
            if (baseTurret.TargetUnit != null && Vector2.DistanceSquared(baseTurret.Position, baseTurret.TargetUnit.Position) > baseTurret.Stats.Range.Total * baseTurret.Stats.Range.Total)
            {
                baseTurret.SetTargetUnit(null, true);
            }
        }

        /// <summary>
        /// Simple target scanning function.
        /// </summary>
        /// TODO: Verify if this needs a rewrite or additions to account for special cases.
        public void CheckForTargets()
        {
            var units = GetUnitsInRange(baseTurret.Position, baseTurret.Stats.Range.Total, true);
            IAttackableUnit nextTarget = null;
            var nextTargetPriority = ClassifyUnit.DEFAULT;

            foreach (var u in units)
            {
                if (u.IsDead || u.Team == baseTurret.Team || !u.Status.HasFlag(StatusFlags.Targetable))
                {
                    continue;
                }

                // Note: this method means that if there are two champions within turret range,
                // The player to have been added to the game first will always be targeted before the others
                if (baseTurret.TargetUnit == null)
                {
                    var priority = baseTurret.ClassifyTarget(u);
                    if (priority < nextTargetPriority)
                    {
                        nextTarget = u;
                        nextTargetPriority = priority;
                    }
                }
                else
                {
                    // Is the current target a champion? If it is, don't do anything
                    if (baseTurret.TargetUnit is IChampion)
                    {
                        continue;
                    }
                    // Find the next champion in range targeting an enemy champion who is also in range
                    if (!(u is IChampion enemyChamp) || enemyChamp.TargetUnit == null)
                    {
                        continue;
                    }
                    if (!(enemyChamp.TargetUnit is IChampion enemyChampTarget) ||
                        Vector2.DistanceSquared(enemyChamp.Position, enemyChampTarget.Position) > enemyChamp.Stats.Range.Total * enemyChamp.Stats.Range.Total ||
                        Vector2.DistanceSquared(baseTurret.Position, enemyChampTarget.Position) > baseTurret.Stats.Range.Total * baseTurret.Stats.Range.Total)
                    {
                        continue;
                    }

                    nextTarget = enemyChamp; // No priority required
                    break;
                }
            }

            if (nextTarget == null)
            {
                return;
            }

            baseTurret.SetTargetUnit(nextTarget, true);
        }


    }
}