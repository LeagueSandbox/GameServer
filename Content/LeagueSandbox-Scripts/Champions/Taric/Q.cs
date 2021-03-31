using System;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class Imbue : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            spell.SpellAnimation("SPELL1", owner);
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            if (target.Team == owner.Team)
            {
                var p1 = AddParticleTarget(owner, "Imbue_glow.troy", target, 1);
                var p2 = AddParticleTarget(owner, "Imbue_cas.troy", owner, 1);
                CreateTimer(1.75f, () =>
                {
                    RemoveParticle(p1);
                    RemoveParticle(p2);
                });
                if (target != owner)
                {
                    PerformHeal(owner, spell, target);
                    PerformHeal(owner, spell, owner);
                }
                else
                {
                    PerformHeal(owner, spell, owner);
                }
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
        }

        private void PerformHeal(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var ap = owner.Stats.AbilityPower.Total * 0.3f;
            var baseHp = (owner.Stats.HealthPoints.Total - owner.Stats.HealthPoints.BaseValue) * 0.05f;
            float healthGain = 20 + spell.CastInfo.SpellLevel * 40 + ap + baseHp;

            if (target == owner && spell.CastInfo.Targets[0] == owner)
            {
                var selfAp = owner.Stats.AbilityPower.Total * 0.42f;
                var selfBaseHp = (owner.Stats.HealthPoints.Total - owner.Stats.HealthPoints.BaseValue) * 0.07f;
                healthGain = 28 + spell.CastInfo.SpellLevel * 56 + selfAp + selfBaseHp;
            }

            var newHealth = target.Stats.CurrentHealth + healthGain;
            target.Stats.CurrentHealth = Math.Min(newHealth, target.Stats.HealthPoints.Total);
            AddParticleTarget(owner, "global_ss_heal_02.troy", target);
            AddParticleTarget(owner, "global_ss_heal_speedboost.troy", target);
        }

        public void OnUpdate(float diff)
        {
        }

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }
    }
}

