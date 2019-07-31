using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class GarenE : IGameScript
    {
        public void OnActivate(IChampion owner)
        {
        }

        public void OnDeactivate(IChampion owner)
        {
        }

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var p = AddParticleTarget(owner, "Garen_Base_E_Spin.troy", owner, 1);
            var visualBuff = AddBuffHudVisual("GarenE", 3.0f, 1,
                BuffType.COMBAT_ENCHANCER, owner, 3.0f);
            CreateTimer(3.0f, () =>
            {
                RemoveParticle(p);
            });
            for (var i = 0.0f; i < 3.0; i += 0.5f)
            {
                CreateTimer(i, () => { ApplySpinDamage(owner, spell, target); });
            }
        }

        private void ApplySpinDamage(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var units = GetUnitsInRange(owner, 500, true);
            foreach (var unit in units)
            {
                if (unit.Team != owner.Team)
                {
                    //PHYSICAL DAMAGE PER SECOND: 20 / 45 / 70 / 95 / 120 (+ 70 / 80 / 90 / 100 / 110% AD)
                    var ad = new[] {.7f, .8f, .9f, 1f, 1.1f}[spell.Level - 1] * owner.Stats.AttackDamage.Total *
                               0.5f;
                    var damage = new[] {20, 45, 70, 95, 120}[spell.Level - 1] * 0.5f + ad;
                    if (unit is Minion) damage *= 0.75f;
                    unit.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_SPELL,
                        false);
                }
            }
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }
    }
}

