using System;
using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class EzrealTrueshotBarrage : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            AddParticleTarget(owner, "Ezreal_bow_huge.troy", owner, 1, "L_HAND");
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var current = new Vector2(owner.Position.X, owner.Position.Y);
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            var to = Vector2.Normalize(spellPos - current);
            var range = to * 20000;
            var trueCoords = current + range;

            spell.AddProjectile("EzrealTrueshotBarrage", new Vector2(spell.CastInfo.SpellCastLaunchPosition.X, spell.CastInfo.SpellCastLaunchPosition.Z), current, trueCoords, HitResult.HIT_Normal, true);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            var reduc = Math.Min(projectile.ObjectsHit.Count, 7);
            var bonusAd = owner.Stats.AttackDamage.Total - owner.Stats.AttackDamage.BaseValue;
            var ap = owner.Stats.AbilityPower.Total * 0.9f;
            var damage = 200 + spell.CastInfo.SpellLevel * 150 + bonusAd + ap;
            target.TakeDamage(owner, damage * (1 - reduc / 10), DamageType.DAMAGE_TYPE_MAGICAL,
                DamageSource.DAMAGE_SOURCE_SPELL, false);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
