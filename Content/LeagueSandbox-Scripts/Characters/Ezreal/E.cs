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
    public class EzrealArcaneShift : ISpellScript
    {
        private ObjAIBase _owner;
        private Spell _spell;
        private SpellSector _sector;

        private const float CAST_RANGE = 475;
        
        private const string CAST_PARTICLE = "Ezreal_arcaneshift_cas";
        private const string CAST_FLASH_PARTICLE = "Ezreal_arcaneshift_flash";

        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            CastingBreaksStealth = true,
            DoesntBreakShields = true,
            TriggersSpellCasts = true,
            IsDamagingSpell = true,
        };

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnSpellPostCast(Spell spell)
        {
            _owner = spell.CastInfo.Owner;
            _spell = spell;
            var startPos = _owner.Position;
            var trueCoords = new Vector2(_spell.CastInfo.TargetPosition.X, _spell.CastInfo.TargetPosition.Z);

            var to = trueCoords - startPos;
            if (to.Length() > CAST_RANGE)
            {
                trueCoords = GetPointFromUnit(_owner, CAST_RANGE);
            }

            var sectorParams = new SectorParameters
            {
                Length = CAST_RANGE,
                Type = SectorType.Area,
                SingleTick = true,
                CanHitSameTarget = false,
                CanHitSameTargetConsecutively = false,
                MaximumHits = 0
            };
            _sector = _spell.CreateSpellSector(sectorParams);

            AddParticle(_owner, null, CAST_PARTICLE, startPos);
            AddParticleTarget(_owner, _owner, CAST_FLASH_PARTICLE, _owner);

            TeleportTo(_owner, trueCoords.X, trueCoords.Y);
        }

        public void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
        {
            if (_owner == null || _sector == null) 
                return;
            
            if (_sector.ObjectsHit.Count == 0)
                return;
            
            _sector.ExecuteTick();
            
            foreach (var targetObj in _sector.ObjectsHit)
            {
                var targetUnit = targetObj as AttackableUnit;
                if (targetUnit == null)
                    continue;
                    
                var castPosition = new Vector2(_spell.CastInfo.TargetPosition.X, _spell.CastInfo.TargetPosition.Z);
                SpellCast(_owner, 1, SpellSlotType.ExtraSlots, true, targetUnit, castPosition);
                break;
                
            }
            _sector.ObjectsHit.Clear();
        }
    }
    public class EzrealArcaneShiftMissile : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Target
            }
        };

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
        {
            target.TakeDamage(spell.CastInfo.Owner, 75f + ((spell.CastInfo.SpellLevel - 1) * 50f) + spell.CastInfo.Owner.Stats.AbilityPower.Total * 0.75f,
                DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            missile.SetToRemove();
        }
    }
}
