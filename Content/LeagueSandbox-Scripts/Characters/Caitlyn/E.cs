using System.Numerics;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using Buffs;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;

namespace Spells
{
    public class CaitlynEntrapment : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            DoesntBreakShields = true,
            TriggersSpellCasts = true,
            NotSingleTargetSpell = false,
            SpellDamageRatio = 1.0f
        };

        public void OnSpellPostCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);

            FaceDirection(spellPos, owner, true);

            var misPos = GetPointFromUnit(owner, spell.SpellData.CastRangeDisplayOverride);
            SpellCast(owner, 1, SpellSlotType.ExtraSlots, misPos, misPos, true, Vector2.Zero);

            if (owner.CanMove())
            {
                AddBuff("CaitlynEntrapment", 0.25f, 1, spell, owner, owner);

                // Distance is 500 - 10 = 490 (a point 10 units in front is used for MoveAway, decreasing the effective range by 10)
                ForceMovement(owner, "", GetPointFromUnit(owner, 490f, 180f), 1000f, 0.0f, 3.0f, 0.0f, movementOrdersFacing: ForceMovementOrdersFacing.KEEP_CURRENT_FACING);
            }
        }
    }

    public class CaitlynEntrapmentMissile : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            DoesntBreakShields = true,
            TriggersSpellCasts = true,
            NotSingleTargetSpell = false,
            SpellDamageRatio = 1.0f,
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Circle
            }
        };

        //Vector2 direction;

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
        {
            var owner = spell.CastInfo.Owner;

            if (!target.Status.HasFlag(StatusFlags.Stealthed))
            {
                // BreakSpellShields(target);

                var slowDuration = new[] { 0, 1, 1.25f, 1.5f, 1.75f, 2 }[spell.CastInfo.SpellLevel];
                var slowBuffScript = AddBuff("Slow", slowDuration, 1, spell, target, owner).BuffScript as Slow;
                slowBuffScript.SetSlowMod(0.5f);

                AddBuff("CaitlynEntrapmentMissile", slowDuration, 1, spell, target, owner);

                var ap = owner.Stats.AbilityPower.Total * 0.8f;
                var damage = 80 + (spell.CastInfo.SpellLevel - 1) * 50 + ap;
                target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);

                AddParticleTarget(owner, target, "caitlyn_entrapment_tar", target);

                missile.SetToRemove();
            }
            else
            {
                if (target is Champion)
                {
                    // BreakSpellShields(target);

                    var slowDuration = new[] { 0, 1, 1.25f, 1.5f, 1.75f, 2 }[spell.CastInfo.SpellLevel];
                    var slowBuffScript = AddBuff("Slow", slowDuration, 1, spell, target, owner).BuffScript as Slow;
                    slowBuffScript.SetSlowMod(0.5f);

                    AddBuff("CaitlynEntrapmentMissile", slowDuration, 1, spell, target, owner);

                    var ap = owner.Stats.AbilityPower.Total * 0.8f;
                    var damage = 80 + (spell.CastInfo.SpellLevel - 1) * 50 + ap;
                    target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);

                    AddParticleTarget(owner, target, "caitlyn_entrapment_tar", target);

                    missile.SetToRemove();
                }
                // TODO: Implement a CanSee function for specific unit->unit vision checking (things such as blinds need this)
                else if (TeamHasVision(owner.Team, target))
                {
                    // BreakSpellShields(target);

                    var slowDuration = new[] { 0, 1, 1.25f, 1.5f, 1.75f, 2 }[spell.CastInfo.SpellLevel];
                    var slowBuffScript = AddBuff("Slow", slowDuration, 1, spell, target, owner).BuffScript as Slow;
                    slowBuffScript.SetSlowMod(0.5f);

                    AddBuff("CaitlynEntrapmentMissile", slowDuration, 1, spell, target, owner);

                    var ap = owner.Stats.AbilityPower.Total * 0.8f;
                    var damage = 80 + (spell.CastInfo.SpellLevel - 1) * 50 + ap;
                    target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);

                    AddParticleTarget(owner, target, "caitlyn_entrapment_tar", target);

                    missile.SetToRemove();
                }
            }
        }
    }
}
