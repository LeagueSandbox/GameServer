using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class Disintegrate : IGameScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            spell.AddProjectileTarget("Disintegrate", target, HitResult.HIT_Normal, false);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            var ap = owner.Stats.AbilityPower.Total * 0.8f;
            var damage = 45 + spell.CastInfo.SpellLevel * 35 + ap;
            if (target != null && !target.IsDead)
            {
                target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL,
                    false);
                if (target.IsDead)
                {
                    spell.LowerCooldown(spell.GetCooldown());
                    float manaToRecover = 55 + spell.CastInfo.SpellLevel * 5;
                    var newMana = owner.Stats.CurrentMana + manaToRecover;
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
            }

            projectile.SetToRemove();
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
