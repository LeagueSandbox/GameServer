using System;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;

namespace Spells
{
    public class Parley : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnSpellPostCast(Spell spell)
        {
            //spell.AddProjectileTarget("pirate_parley_mis", spell.CastInfo.SpellCastLaunchPosition, target);
        }

        public void ApplyEffects(ObjAIBase owner, AttackableUnit target, Spell spell, SpellMissile missile)
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
                    if(owner is Champion ch)
                    {
                        ch.AddGold(ch, goldIncome);

                    }
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
    }
}
