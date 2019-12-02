using System;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class SummonerHeal : IGameScript
    {
        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var units = GetChampionsInRange(owner, 850, true);
            units.Remove(owner);
            IChampion mostWoundedAlliedIChampion = null;
            float lowestHealthPercentage = 100;
            float maxHealth;
            foreach(var value in units) {
                if (owner.Team == value.Team)
                {
                    var currentHealth = value.Stats.CurrentHealth;
                    maxHealth = value.Stats.HealthPoints.Total;
                    if (currentHealth * 100 / maxHealth < lowestHealthPercentage && owner != value)
                    {
                        lowestHealthPercentage = currentHealth * 100 / maxHealth;
                        mostWoundedAlliedIChampion = value;
                    }
                }
            }

            if (mostWoundedAlliedIChampion != null)
            {
                PerformHeal(owner, spell, mostWoundedAlliedIChampion);
            }

            PerformHeal(owner, spell, owner);
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        private void PerformHeal(IChampion owner, ISpell spell, IChampion target)
        {
            float healthGain = 75 + (target.Stats.Level * 15);
            if (target.HasBuffGameScriptActive("HealCheck", "HealCheck"))
            {
                healthGain *= 0.5f;
            }
            var newHealth = target.Stats.CurrentHealth + healthGain;
            target.Stats.CurrentHealth = Math.Min(newHealth, target.Stats.HealthPoints.Total);
            target.AddBuffGameScript("HealSpeed", "HealSpeed", spell, 1.0f, true);
            target.AddBuffGameScript("HealCheck", "HealCheck", spell, 35.0f, true);
            AddParticleTarget(owner, "global_ss_heal_02.troy", target);
            AddParticleTarget(owner, "global_ss_heal_speedboost.troy", target);
        }

        public void OnUpdate(double diff)
        {
        }

        public void OnActivate(IChampion owner)
        {
        }

        public void OnDeactivate(IChampion owner)
        {
        }
    }
}

