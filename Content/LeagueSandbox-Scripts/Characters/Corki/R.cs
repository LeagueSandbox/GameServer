using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;

namespace Spells
{
    public class MissileBarrage : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            PersistsThroughDeath = true,
            IsNonDispellable = true,
            TriggersSpellCasts = true,
            CooldownIsAffectedByCDR = false,
            SpellFXOverrideSkins = new string[]
            {
                "FireworksCorki"
            }
        };

        public void OnSpellCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            FaceDirection(new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z), owner, true);
            var targetPos = GetPointFromUnit(spell.CastInfo.Owner, spell.SpellData.CastRange[0]);
            SpellCast(owner, 1, SpellSlotType.ExtraSlots, owner.Position, targetPos, false, Vector2.Zero);
        }
    }

    public class MissileBarrageMissile : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Circle
            },
            DoesntBreakShields = false,
            TriggersSpellCasts = false,
            SpellFXOverrideSkins = new string[]
            {
                "FireworksCorki"
            }
        };


        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, OnSpellHit, false);
        }

        public void OnSpellHit(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
        {
            var owner = spell.CastInfo.Owner;

            var spellLevel = owner.Spells[3].CastInfo.SpellLevel;
            float AP = owner.Stats.AbilityPower.Total * 0.3f;
            float AD = owner.Stats.AttackDamage.Total * (0.1f + (0.1f * spellLevel));
            float totalDamage = 20 + (80 * spellLevel) + AP + AD;

            var targets = GetUnitsInRange(target.Position, 75.0f, true);
            foreach (var unit in targets)
            {
                unit.TakeDamage(owner, totalDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELLAOE, false);
            }

            AddParticle(owner, null, "corki_misslebarrage_std_tar", target.Position);

            missile.SetToRemove();
        }

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            FaceDirection(end, owner, true);
        }
    }
}
