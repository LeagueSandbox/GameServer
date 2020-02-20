using System;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class SummonerHeal : IGameScript
    {
        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var units = GetChampionsInRange(owner, 850, true);
            IChampion mostWoundedAlliedIChampion = null;
            float lowestHealthPercentage = 100;
            float maxHealth;
            foreach (var value in units)
            {
                if (value != owner && owner.Team == value.Team)
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

            PerformHeal(owner, spell, (IChampion)owner);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        private void PerformHeal(IObjAiBase owner, ISpell spell, IChampion target)
        {
            float healthGain = 75 + (target.Stats.Level * 15);
            if (target.HasBuff("HealCheck"))
            {
                healthGain *= 0.5f;
            }
            var newHealth = target.Stats.CurrentHealth + healthGain;
            target.Stats.CurrentHealth = Math.Min(newHealth, target.Stats.HealthPoints.Total);
            AddBuff("HealSpeed", 1.0f, 1, spell, target, owner);
            AddBuff("HealCheck", 35.0f, 1, spell, target, owner);
            AddParticleTarget(owner, "global_ss_heal_02.troy", target);
            AddParticleTarget(owner, "global_ss_heal_speedboost.troy", target);
        }

        public void OnUpdate(double diff)
        {
        }

        public void OnActivate(IObjAiBase owner)
        {
        }

        public void OnDeactivate(IObjAiBase owner)
        {
        }
    }
}

