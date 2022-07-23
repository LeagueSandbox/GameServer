using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;
using LeagueSandbox.GameServer.GameObjects;

namespace Spells
{
    public class GlacialStorm : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            CastingBreaksStealth = true,
            DoesntBreakShields = true,
            IsDamagingSpell = true,
            NotSingleTargetSpell = true,
            SpellToggleSlot = 4
        };

        Buff thisBuff;
        public SpellSector DamageSector;
        public SpellSector SlowSector;

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnSpellPostCast(Spell spell)
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

        public void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
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
    }
}
