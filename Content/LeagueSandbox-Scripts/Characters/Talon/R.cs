using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using static GameServerCore.Domain.GameObjects.IGameObject;
using GameServerCore;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace Spells
{
    public class TalonShadowAssault : ISpellScript
    {
		float timeSinceLastTick = 1000f;
		public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();
		IBuff HandlerBuff;
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true
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
            var owner = spell.CastInfo.Owner as IChampion;
            PlayAnimation(owner, "Spell4");
			AddBuff("TalonShadowAssaultBuff", 2.5f, 1, spell, owner, owner, false);			
            for (int bladeCount = 0; bladeCount <= 7; bladeCount++)
            {				           
                var start = GetPointFromUnit(owner, 25f, bladeCount * 45f);
				var end = GetPointFromUnit(owner, 615f, bladeCount * 45f);
				SpellCast(owner, 3, SpellSlotType.ExtraSlots, end, Vector2.Zero, true, start);
				SpellCast(owner, 5, SpellSlotType.ExtraSlots, end, Vector2.Zero, true, start);			            			
            }			
            AddParticleTarget(owner, owner, "talon_ult_cas.troy", owner, 10f);
        }
		public void SetSpellToggle(bool toggle)
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

    public class TalonShadowAssaultMisOne : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Circle
            },
            IsDamagingSpell = true,
            TriggersSpellCasts = true
        };
        public List<IAttackableUnit> UnitsHit = new List<IAttackableUnit>();

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            UnitsHit.Clear();
        }

        public void OnMissileEnd(ISpellMissile missile)
        {           
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            var owner = spell.CastInfo.Owner;
            var ADratio = owner.Stats.AttackDamage.Total * 0.75f;
            var damage = 120 + 50f * (spell.CastInfo.SpellLevel - 1) + ADratio;
			var ELevel = owner.GetSpell("TalonCutthroat").CastInfo.SpellLevel;
			var damageamp = 0.03f * (ELevel - 1);
			if (target.HasBuff("TalonDamageAmp"))
            {
				damage = damage + damage * damageamp;
            }
            if (!UnitsHit.Contains(target) && target != spell.CastInfo.Owner)
            {
				if (target.Team != owner.Team && !(target is IObjBuilding || target is IBaseTurret))
				{
                UnitsHit.Add(target);
                target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
                AddParticleTarget(owner, target, "talon_ult_tar.troy", target, 1f);
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
	public class TalonShadowAssaultMisOneHalf : ISpellScript
    {
		ISpell spell;
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Circle
            },
            IsDamagingSpell = true,
            TriggersSpellCasts = true

            // TODO
        };
        public List<IAttackableUnit> UnitsHit = new List<IAttackableUnit>();

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            
        }
        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            UnitsHit.Clear();
            var missile = spell.CreateSpellMissile(new MissileParameters
            {
                Type = MissileType.Circle,
                OverrideEndPosition = end
            });

            ApiEventManager.OnSpellMissileEnd.AddListener(this, missile, OnMissileEnd, true);
        }

        public void OnMissileEnd(ISpellMissile missile)
        {
            var owner = missile.CastInfo.Owner;
            //owner.GetSpell("TalonShadowAssaultToggle").SetCooldown(0f);			
            IMinion Blade = AddMinion(owner, "TestCube", "TestCube", missile.Position, owner.Team, owner.SkinID, true, false);
			AddBuff("TalonShadowAssaultMarker", 2.24f, 1, spell, Blade, Blade, false);
        }
        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {            
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
    public class TalonShadowAssaultMisTwo : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Circle
            },
            IsDamagingSpell = true,
            TriggersSpellCasts = true
        };
        public List<IAttackableUnit> UnitsHit = new List<IAttackableUnit>();

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            UnitsHit.Clear();
            var missile = spell.CreateSpellMissile(new MissileParameters
            {
                Type = MissileType.Circle,
                OverrideEndPosition = end
            });
        }
     
        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
			if (target == missile.CastInfo.Owner)
            {
				missile.SetToRemove();            
            }
            var owner = spell.CastInfo.Owner;         
            var ADratio = owner.Stats.AttackDamage.Total * 0.75f;
            var damage = 120 + 50f * (spell.CastInfo.SpellLevel - 1) + ADratio;
			var ELevel = owner.GetSpell("TalonCutthroat").CastInfo.SpellLevel;
			var damageamp = 0.03f * (ELevel - 1);
			if (target.HasBuff("TalonDamageAmp"))
            {
				damage = damage + damage * damageamp;
            }

            if (!UnitsHit.Contains(target) && target != spell.CastInfo.Owner)
            {
				if (target.Team != owner.Team && !(target is IObjBuilding || target is IBaseTurret))
				{
                UnitsHit.Add(target);
                target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
                AddParticleTarget(owner, target, "talon_ult_tar.troy", target, 1f);
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
	public class TalonShadowAssaultToggle : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true
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

