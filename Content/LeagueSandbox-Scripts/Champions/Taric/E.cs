using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class Dazzle : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
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

        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            if (Vector2.DistanceSquared(owner.Position, target.Position) > 625 * 625)
            {
                return;
            }
            else
            {
                spell.AddProjectileTarget("Dazzle", spell.CastInfo.SpellCastLaunchPosition, target, HitResult.HIT_Normal, true);
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            var time = 1.1f + 0.1f * spell.CastInfo.SpellLevel;
            var ap = owner.Stats.AbilityPower.Total;
            var damage = 10 + spell.CastInfo.SpellLevel * 30 + ap * 0.2f;
            var dist = Vector2.DistanceSquared(owner.Position, target.Position);
            if (dist <= 460 * 460)
            {
                damage = 15 + spell.CastInfo.SpellLevel * 45 + ap * 0.3f;
            }
            if (dist <= 295 * 295)
            {
                damage = 20 + spell.CastInfo.SpellLevel * 60 + ap * 0.4f;
            }

            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);

            AddBuff("TaricEHud", time, 1, spell, target, owner);
            AddBuff("Stun", time, 1, spell, target, owner);
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

        public void OnUpdate(float diff)
        {
        }
    }
}
