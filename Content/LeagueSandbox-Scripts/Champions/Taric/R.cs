using System.Linq;
using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace Spells
{
    public class TaricHammerSmash : IGameScript
    {
        public void OnActivate(IObjAiBase owner)
        {
        }

        public void OnDeactivate(IObjAiBase owner)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {

        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var p1 = AddParticleTarget(owner, "TaricHammerSmash_nova.troy", owner);
            var p2 = AddParticleTarget(owner, "TaricHammerSmash_shatter.troy", owner);
            var hasbuff = owner.HasBuff("Radiance");
            var ap = owner.Stats.AbilityPower.Total * 0.5f;
            var damage = 50 + spell.Level * 100 + ap;

            foreach (var enemyTarget in GetUnitsInRange(owner, 375, true)
                .Where(x => x.Team == CustomConvert.GetEnemyTeam(owner.Team)))
            {
                if (enemyTarget is IAttackableUnit)
                {
                    enemyTarget.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                    var ep1 = AddParticleTarget(owner, "Taric_GemStorm_Tar.troy", enemyTarget, 1.25f);
                    var ep2 = AddParticleTarget(owner, "Taric_GemStorm_Aura.troy", enemyTarget, 1.25f);
                    var ep3 = AddParticleTarget(owner, "Taric_ShoulderFlare.troy", enemyTarget, 1.25f);
                    CreateTimer(1f, () =>
                    {
                        RemoveParticle(ep1);
                        RemoveParticle(ep2);
                        RemoveParticle(ep3);
                    });
                }
            }

            foreach (var allyTarget in GetUnitsInRange(owner, 1100, true)
                .Where(x => x.Team != CustomConvert.GetEnemyTeam(owner.Team)))
            {
                if (allyTarget is IObjAiBase && owner != allyTarget && hasbuff == false)
                {
                    AddBuff("Radiance_ally", 10.0f, 1, spell, (IObjAiBase)allyTarget, owner);
                }
            }
            if (owner == target && hasbuff == false)
            {
                var p3 = AddParticleTarget(owner, "taricgemstorm.troy", owner);
                AddBuff("Radiance", 10.0f, 1, spell, owner, owner);
                CreateTimer(10.0f, () =>
                {
                    RemoveParticle(p3);
                });
            }

            CreateTimer(10.0f, () =>
            {
                RemoveParticle(p1);
                RemoveParticle(p2);
            }
            );
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {

        }

        public void OnUpdate(double diff)
        {
        }
    }
}
