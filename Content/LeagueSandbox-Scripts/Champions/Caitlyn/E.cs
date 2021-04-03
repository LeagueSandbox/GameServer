using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class CaitlynEntrapment : IGameScript
    {
        public void OnActivate(IObjAiBase owner)
        {
        }

        public void OnDeactivate(IObjAiBase owner)
        {
        }

        public void OnUpdate(double diff)
        {

        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            // Calculate net coords
            var current = new Vector2(owner.Position.X, owner.Position.Y);
            var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - current);
            var range = to * 750;
            var trueCoords = current + range;

            // Calculate dash coords/vector
            var dash = Vector2.Negate(to) * 500;
            var dashCoords = current + dash;
            ForceMovement(owner, "Spell3", dashCoords, 1000, 0, 0, 0, movementOrdersFacing: ForceMovementOrdersFacing.KEEP_CURRENT_FACING);
            spell.AddProjectile("CaitlynEntrapmentMissile", current, trueCoords);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            var ap = owner.Stats.AbilityPower.Total * 0.8f;
            var damage = 80 + (spell.Level - 1) * 50 + ap;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            var slowDuration = new[] { 0, 1, 1.25f, 1.5f, 1.75f, 2 }[spell.Level];
            AddBuff("Slow", slowDuration, 1, spell, (IObjAiBase)target, owner);
            AddParticleTarget(owner, "caitlyn_entrapment_tar.troy", target);
            AddParticleTarget(owner, "caitlyn_entrapment_slow.troy", target);
            projectile.SetToRemove();
        }
    }
}
