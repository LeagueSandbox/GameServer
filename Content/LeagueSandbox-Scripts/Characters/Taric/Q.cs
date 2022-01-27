using System;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;

namespace Spells
{
    public class Imbue : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
            //spell.CastInfo.Owner.SpellAnimation("SPELL1");
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            var target = spell.CastInfo.Targets[0].Unit;
            if (target.Team == owner.Team)
            {
                var p1 = AddParticleTarget(owner, target, "Imbue_glow", target);
                var p2 = AddParticleTarget(owner, owner, "Imbue_cas", owner);
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
            AddParticleTarget(owner, target, "global_ss_heal_02", target);
            AddParticleTarget(owner, target, "global_ss_heal_speedboost", target);
        }

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
        }

        public void OnSpellPostChannel(ISpell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

