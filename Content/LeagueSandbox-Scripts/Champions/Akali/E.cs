using System.Linq;
using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class AkaliShadowSwipe : IGameScript
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
            var ap = owner.Stats.AbilityPower.Total * 0.3f;
            var ad = owner.Stats.AttackDamage.Total * 0.6f;
            var damage = 40 + spell.Level * 30 + ap + ad;
            foreach (var enemyTarget in GetUnitsInRange(owner, 300, true)
                .Where(x => x.Team == CustomConvert.GetEnemyTeam(owner.Team)))
            {
                enemyTarget.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL,
                    false);
                if (target.IsDead)
                {
                    spell.LowerCooldown(spell.CurrentCooldown * 0.6f);
                }
            }
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {

        }

        public void OnUpdate(double diff)
        {
        }
    }
}
