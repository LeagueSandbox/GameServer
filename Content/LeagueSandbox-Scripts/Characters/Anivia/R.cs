using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using System.Collections.Generic;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class GlacialStorm : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            CastingBreaksStealth = true,
            DoesntBreakShields = true,
            IsDamagingSpell = true,
            NotSingleTargetSpell = true,
            SpellToggleSlot = 4
        };

        IBuff thisBuff;
        public ISpellSector DamageSector;
        public ISpellSector SlowSector;

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

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;

            if (owner.HasBuff("GlacialStorm"))
            {
                owner.RemoveBuffsWithName("GlacialStorm");
            }
            else
            {
                thisBuff = AddBuff("GlacialStorm", 25000.0f, 1, spell, owner, owner);

                DamageSector = spell.CreateSpellSector(new SectorParameters
                {
                    Length = 400f,
                    Tickrate = 2,
                    CanHitSameTargetConsecutively = true,
                    OverrideFlags = SpellDataFlags.AffectEnemies | SpellDataFlags.AffectNeutral | SpellDataFlags.AffectMinions | SpellDataFlags.AffectHeroes,
                    Type = SectorType.Area
                });

                SlowSector = spell.CreateSpellSector(new SectorParameters
                {
                    Length = 400f,
                    Tickrate = 4,
                    CanHitSameTargetConsecutively = true,
                    OverrideFlags = SpellDataFlags.AffectEnemies | SpellDataFlags.AffectNeutral | SpellDataFlags.AffectMinions | SpellDataFlags.AffectHeroes,
                    Type = SectorType.Area
                });
            }
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            if (!spell.CastInfo.Owner.HasBuff(thisBuff))
            {
                return;
            }

            if (SlowSector != null && sector == SlowSector)
            {
                AddBuff("Chilled", 1.0f, 1, spell, target, spell.CastInfo.Owner);

                return;
            }

            var ap = spell.CastInfo.Owner.Stats.AbilityPower.Total * 0.125f;
            var damage = 40 + (20 * (spell.CastInfo.SpellLevel - 1)) + ap;

            target.TakeDamage(spell.CastInfo.Owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELLAOE, false);
            AddBuff("Chilled", 1.0f, 1, spell, target, spell.CastInfo.Owner);
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
