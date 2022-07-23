using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;

namespace Spells
{
    public class Dazzle : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnSpellPostCast(Spell spell)
        {
            if (Vector2.DistanceSquared(spell.CastInfo.Owner.Position, spell.CastInfo.Targets[0].Unit.Position) > 625 * 625)
            {
                return;
            }
            else
            {
                //spell.AddProjectileTarget("Dazzle", spell.CastInfo.SpellCastLaunchPosition, spell.CastInfo.Targets[0].Unit, HitResult.HIT_Normal, true);
            }
        }

        public void ApplyEffects(ObjAIBase owner, AttackableUnit target, Spell spell, SpellMissile missile)
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
            AddParticleTarget(owner, target, "Dazzle_tar", target);
            var p103 = AddParticleTarget(owner, target, "Taric_HammerFlare", target);

            CreateTimer(time, () =>
            {
                RemoveParticle(p103);
            });

            missile.SetToRemove();
        }
    }
}
