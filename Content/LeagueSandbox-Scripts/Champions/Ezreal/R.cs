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

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
            AddParticleTarget(spell.CastInfo.Owner, "Ezreal_bow_huge.troy", spell.CastInfo.Owner, 1, "L_HAND", lifetime: 1.0f);
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var current = new Vector2(spell.CastInfo.Owner.Position.X, spell.CastInfo.Owner.Position.Y);
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            var to = Vector2.Normalize(spellPos - current);
            var range = to * 20000;
            var trueCoords = current + range;

            //spell.AddProjectile("EzrealTrueshotBarrage", new Vector2(spell.CastInfo.SpellCastLaunchPosition.X, spell.CastInfo.SpellCastLaunchPosition.Z), current, trueCoords, HitResult.HIT_Normal, true);
        }

        //public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile missile)
        //{
        //    var reduc = Math.Min(missile.ObjectsHit.Count, 7);
        //    var bonusAd = owner.Stats.AttackDamage.Total - owner.Stats.AttackDamage.BaseValue;
        //    var ap = owner.Stats.AbilityPower.Total * 0.9f;
        //    var damage = 200 + spell.CastInfo.SpellLevel * 150 + bonusAd + ap;
        //    target.TakeDamage(owner, damage * (1 - reduc / 10), DamageType.DAMAGE_TYPE_MAGICAL,
        //        DamageSource.DAMAGE_SOURCE_SPELL, false);
        //}

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell)
        {
        }

        public void OnSpellPostChannel(ISpell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
