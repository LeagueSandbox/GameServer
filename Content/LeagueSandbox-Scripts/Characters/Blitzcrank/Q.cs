using System.Numerics;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;

namespace Spells
{
    public class RocketGrab : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            FaceDirection(end, owner);
        }

        public void OnSpellPostCast(Spell spell)
        {
            var endPos = GetPointFromUnit(spell.CastInfo.Owner, 925);
            SpellCast(spell.CastInfo.Owner, 0, SpellSlotType.ExtraSlots, endPos, endPos, false, Vector2.Zero);
        }
    }

    public class RocketGrabMissile : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Circle
            }
            // TODO
        };

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnSpellCast(Spell spell)
        {
            // Prevent flash during cast.
            SealSpellSlot(spell.CastInfo.Owner, SpellSlotType.SummonerSpellSlots, 0, SpellbookType.SPELLBOOK_SUMMONER, true);
            SealSpellSlot(spell.CastInfo.Owner, SpellSlotType.SummonerSpellSlots, 1, SpellbookType.SPELLBOOK_SUMMONER, true);
        }

        public void OnSpellPostCast(Spell spell)
        {
            SealSpellSlot(spell.CastInfo.Owner, SpellSlotType.SummonerSpellSlots, 0, SpellbookType.SPELLBOOK_SUMMONER, false);
            SealSpellSlot(spell.CastInfo.Owner, SpellSlotType.SummonerSpellSlots, 1, SpellbookType.SPELLBOOK_SUMMONER, false);
        }

        public void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
        {
            var owner = spell.CastInfo.Owner;
            var ap = owner.Stats.AbilityPower.Total;
            var damage = 80 + ((spell.CastInfo.SpellLevel - 1) * 55) + ap;
            var dist = System.Math.Abs(Vector2.Distance(target.Position, owner.Position));
            var time = dist / 1350f;

            // Grab particle
            AddBuff("RocketGrab", time, 1, spell, target, owner);

            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);

            AddBuff("Stun", 0.6f, 1, spell, target, owner);

            missile.SetToRemove();

            // SetStatus & auto attack
            AddBuff("RocketGrab2", 0.6f, 1, spell, target, owner);
        }
    }
}

