using System;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Scripting.CSharp;

namespace Spells
{
    public class Parley : ISpellScript
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
        }

        public void OnSpellPostCast(ISpell spell)
        {
            //spell.AddProjectileTarget("pirate_parley_mis", spell.CastInfo.SpellCastLaunchPosition, target);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile missile)
        {
            var isCrit = new Random().Next(0, 100) < owner.Stats.CriticalChance.Total;
            var baseDamage = new[] { 20, 45, 70, 95, 120 }[spell.CastInfo.SpellLevel - 1] + owner.Stats.AttackDamage.Total;
            var damage = isCrit ? baseDamage * owner.Stats.CriticalDamage.Total / 100 : baseDamage;
            var goldIncome = new[] { 4, 5, 6, 7, 8 }[spell.CastInfo.SpellLevel - 1];
            if (target != null && !target.IsDead)
            {
                target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK,
                    false);
                if (target.IsDead)
                {
                    owner.Stats.Gold += goldIncome;
                    var manaCost = new float[] { 50, 55, 60, 65, 70 }[spell.CastInfo.SpellLevel - 1];
                    var newMana = owner.Stats.CurrentMana + manaCost / 2;
                    var maxMana = owner.Stats.ManaPoints.Total;
                    if (newMana >= maxMana)
                    {
                        owner.Stats.CurrentMana = maxMana;
                    }
                    else
                    {
                        owner.Stats.CurrentMana = newMana;
                    }
                }

                missile.SetToRemove();
            }
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
