using LeagueSandbox.GameServer.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.GameObjects.Missiles;
using GameServerCore.Enums;

namespace Spells
{
    public class DeathfireGrasp : IGameScript
    {
        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            spell.AddProjectileTarget("DeathfireGraspSpell",target);
            var p1 = AddParticleTarget(owner, "deathFireGrasp_tar.troy", target);
            var p2 = AddParticleTarget(owner, "obj_DeathfireGrasp_debuff.troy", target);
            AddBuffHudVisual("DeathfireGraspSpell", 4.0f, 1, BuffType.COMBAT_DEHANCER, (ObjAiBase)target, 4.0f);
            CreateTimer(4.0f, () =>
            {
                RemoveParticle(p1);
                RemoveParticle(p2);
            });
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            var damage = target.Stats.HealthPoints.Total * 0.15f;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SUMMONER_SPELL, false);
            projectile.SetToRemove();
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
