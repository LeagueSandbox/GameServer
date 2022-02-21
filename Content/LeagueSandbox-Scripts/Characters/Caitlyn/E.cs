using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using LeagueSandbox.GameServer.API;
using Buffs;

namespace Spells
{
    public class CaitlynEntrapment : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            DoesntBreakShields = true,
            TriggersSpellCasts = true,
            NotSingleTargetSpell = false,
            SpellDamageRatio = 1.0f
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
        }

        public void OnSpellPostCast(ISpell spell)
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

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
        }

        public void OnSpellPostChannel(ISpell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }

    public class CaitlynEntrapmentMissile : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
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

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
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
                if (target is IChampion)
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

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
        }

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
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
