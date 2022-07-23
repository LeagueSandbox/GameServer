using System.Numerics;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;

namespace Spells
{
    public class JavelinToss : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public float finaldamage;
        public Vector2 castcoords;

        public void OnSpellCast(Spell spell)
        {
            castcoords = spell.CastInfo.Owner.Position;
        }

        public void OnSpellPostCast(Spell spell)
        {
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            var to = Vector2.Normalize(spellPos - castcoords);
            var range = to * 1500f;
            var trueCoords = castcoords + range;
            //spell.AddProjectile("JavelinToss", castcoords, castcoords, trueCoords, HitResult.HIT_Normal, true);
        }

        public void ApplyEffects(ObjAIBase owner, AttackableUnit target, Spell spell, SpellMissile missile)
        {
            var ap = owner.Stats.AbilityPower.Total;
            var basedamage = 25 + spell.CastInfo.SpellLevel * 55 + ap;
            var hitcoords = missile.Position;
            var distance = Math.Sqrt(Math.Pow(castcoords.X - hitcoords.X, 2) + Math.Pow(castcoords.Y - hitcoords.Y, 2));
            if (Math.Abs(distance) <= 525f)
            {
                finaldamage = basedamage;
            }
            else if (distance > 525f && !(distance >= 1300f))
            {
                var damagerampup = (basedamage * (0.02f * (float)Math.Round(Math.Abs(distance - 525f) / 7.75f)));
                finaldamage = basedamage + damagerampup;
            }
            else if (distance >= 1300f)
            {
                finaldamage = (basedamage + (basedamage * 2));
            }
            target.TakeDamage(owner, finaldamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            if (!target.IsDead)
            {
                AddParticleTarget(owner, target, "Nidalee_Base_Q_Tar", target, bone: "C_BUFFBONE_GLB_CHEST_LOC");
            }

            missile.SetToRemove();
        }
    }
}

