using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;

namespace Spells
{
    public class EzrealEssenceFlux : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnSpellCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            AddParticleTarget(owner, owner,  "ezreal_bow_yellow", owner, bone: "L_HAND");
        }

        public void OnSpellPostCast(Spell spell)
        {
            var current = spell.CastInfo.Owner.Position;
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            var to = Vector2.Normalize(spellPos - current);
            var range = to * 1000;
            var trueCoords = current + range;
            //spell.AddProjectile("EzrealEssenceFluxMissile", new Vector2(spell.CastInfo.SpellCastLaunchPosition.X, spell.CastInfo.SpellCastLaunchPosition.Z), trueCoords, trueCoords, overrideCastPosition: true);
        }

        public void ApplyEffects(ObjAIBase owner, AttackableUnit target, Spell spell, SpellMissile missile)
        {
            var champion = target as Champion;

            if (champion == null)
            {
                return;
            }

            var buffTime = 5f;
            var ownerAbilityPowerTotal = owner.Stats.AbilityPower.Total;

            if (champion.Team.Equals(owner.Team) && !champion.Equals(owner))
            {
                AddBuff("EzrealWBuff", buffTime, 1, spell, champion, owner);
            }
            else
            {
                var damage = 25 + (45 * spell.CastInfo.SpellLevel) + (ownerAbilityPowerTotal * 0.8f);

                target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, DamageResultType.RESULT_NORMAL);
            }
        }
    }
}
