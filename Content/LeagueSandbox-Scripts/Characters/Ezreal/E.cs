using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell.Sector;

namespace Spells
{
    public class EzrealArcaneShift : ISpellScript
    {
        private IObjAIBase _owner;
        private ISpell _spell;
        private ISpellSector _sector;

        private const float CAST_RANGE = 475;
        
        private const string CAST_PARTICLE = "Ezreal_arcaneshift_cas";
        private const string CAST_FLASH_PARTICLE = "Ezreal_arcaneshift_flash";

        public ISpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            CastingBreaksStealth = true,
            DoesntBreakShields = true,
            TriggersSpellCasts = true,
            IsDamagingSpell = true,
        };

        public void OnActivate(IObjAIBase owner, ISpell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnDeactivate(IObjAIBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAIBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
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

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
        }

        public void OnSpellPostChannel(ISpell spell)
        {
        }
        
        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            if (_owner == null || _sector == null) 
                return;
            
            if (_sector.ObjectsHit.Count == 0)
                return;
            
            _sector.ExecuteTick();
            
            foreach (var targetObj in _sector.ObjectsHit)
            {
                var targetUnit = targetObj as IAttackableUnit;
                if (targetUnit == null)
                    continue;
                    
                var castPosition = new Vector2(_spell.CastInfo.TargetPosition.X, _spell.CastInfo.TargetPosition.Z);
                SpellCast(_owner, 1, SpellSlotType.ExtraSlots, true, targetUnit, castPosition);
                break;
                
            }
            _sector.ObjectsHit.Clear();
        }

        public void OnUpdate(float diff)
        {
        }
    }
    public class EzrealArcaneShiftMissile : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Target
            }
        };

        public void OnActivate(IObjAIBase owner, ISpell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnDeactivate(IObjAIBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAIBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            target.TakeDamage(spell.CastInfo.Owner, 75f + ((spell.CastInfo.SpellLevel - 1) * 50f) + spell.CastInfo.Owner.Stats.AbilityPower.Total * 0.75f,
                DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            missile.SetToRemove();
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
