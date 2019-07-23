using System;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Missiles;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class Parley : IGameScript
    {
        public void OnActivate(IChampion owner)
        {
        }

        public void OnDeactivate(IChampion owner)
        {
        }

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            spell.AddProjectileTarget("pirate_parley_mis", target);
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            var isCrit = new Random().Next(0, 100) < owner.Stats.CriticalChance.Total;
            var baseDamage = new[] {20, 45, 70, 95, 120}[spell.Level - 1] + owner.Stats.AttackDamage.Total;
            var damage = isCrit ? baseDamage * owner.Stats.CriticalDamage.Total / 100 : baseDamage;
            var goldIncome = new[] {4, 5, 6, 7, 8}[spell.Level - 1];
            if (target != null && !target.IsDead)
            {
                target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK,
                    false);
                if (target.IsDead)
                {
                    owner.Stats.Gold += goldIncome;
                    var manaCost = new float[] {50, 55, 60, 65, 70}[spell.Level - 1];
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

                projectile.SetToRemove();
            }
        }

        public void OnUpdate(double diff)
        {
        }
    }
}
