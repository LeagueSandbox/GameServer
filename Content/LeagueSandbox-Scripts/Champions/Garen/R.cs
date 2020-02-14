using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class GarenR : IGameScript
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
            AddParticleTarget(owner, "Garen_Base_R_Tar_Impact.troy", target, 1);
            AddParticleTarget(owner, "Garen_Base_R_Sword_Tar.troy", target, 1);
            var missinghealth = target.Stats.HealthPoints.Total - target.Stats.CurrentHealth;
            var damageperc = missinghealth * new[] { 0.28f, 0.33f, 0.40f }[spell.Level - 1];
            var damage = spell.Level * 175 + damageperc;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            if (target.IsDead)
            {
                AddParticleTarget(owner, "Garen_Base_R_Champ_Kill.troy", target, 1);
                AddParticleTarget(owner, "Garen_Base_R_Champ_Death.troy", target, 1);
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }
    }
}
