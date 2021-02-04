using System;
using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class CaitlynPiltoverPeacemaker : IGameScript
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
            var current = new Vector2(owner.Position.X, owner.Position.Y);
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            var to = Vector2.Normalize(spellPos - current);
            var range = to * 1150;
            var trueCoords = current + range;
            spell.AddProjectile("CaitlynPiltoverPeacemaker", current, trueCoords, HitResult.HIT_Normal, true);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            var reduc = Math.Min(projectile.ObjectsHit.Count, 5);
            var baseDamage = new[] { 20, 60, 100, 140, 180 }[spell.CastInfo.SpellLevel - 1] +
                             1.3f * owner.Stats.AttackDamage.Total;
            var damage = baseDamage * (1 - reduc / 10);
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
