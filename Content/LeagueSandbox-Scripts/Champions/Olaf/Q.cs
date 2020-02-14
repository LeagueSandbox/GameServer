using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class OlafAxeThrowCast : IGameScript
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
            var current = new Vector2(owner.X, owner.Y);
            var to = new Vector2(spell.X, spell.Y) - current;
            Vector2 trueCoords;

            if (to.Length() > 1651)
            {
                to = Vector2.Normalize(to);
                var range = to * 1651;
                trueCoords = current + range;
            }
            else
            {
                trueCoords = new Vector2(spell.X, spell.Y);
            }

            spell.AddProjectile("OlafAxeThrowDamage", owner.X, owner.Y, trueCoords.X, trueCoords.Y);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            AddParticleTarget(owner, "olaf_axeThrow_tar.troy", target, 1);
            var ad = owner.Stats.AttackDamage.Total * 1.1f;
            var ap = owner.Stats.AttackDamage.Total * 0.0f;
            var damage = 15 + spell.Level * 20 + ad + ap;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
        }

        public void OnUpdate(double diff)
        {
        }
    }
}

