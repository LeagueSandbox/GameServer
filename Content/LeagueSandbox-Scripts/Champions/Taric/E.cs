using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class Dazzle : IGameScript
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
            if (owner.GetDistanceTo(target) > 625)
            {
                return;
            }
            else
            {
                spell.AddProjectileTarget("Dazzle", target, true);
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            var time = 1.1f + 0.1f * spell.Level;
            var ap = owner.Stats.AbilityPower.Total;
            var damage = 10 + spell.Level * 30 + ap * 0.2f;
            if (owner.GetDistanceTo(target) <= 460)
            {
                damage = 15 + spell.Level * 45 + ap * 0.3f;
            }
            if (owner.GetDistanceTo(target) <= 295)
            {
                damage = 20 + spell.Level * 60 + ap * 0.4f;
            }

            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);

            AddBuff("TaricEHud", time, 1, spell, (IObjAiBase)target, owner);
            AddBuff("Stun", time, 1, spell, (IObjAiBase)target, owner);
            AddParticleTarget(owner, "Dazzle_tar.troy", target);
            var p102 = AddParticleTarget(owner, "Global_Stun.troy", target, 1.25f, "head");
            var p103 = AddParticleTarget(owner, "Taric_HammerFlare.troy", target, 1);

            CreateTimer(time, () =>
            {
                RemoveParticle(p102);
                RemoveParticle(p103);
            });

            projectile.SetToRemove();
        }

        public void OnUpdate(double diff)
        {
        }
    }
}
