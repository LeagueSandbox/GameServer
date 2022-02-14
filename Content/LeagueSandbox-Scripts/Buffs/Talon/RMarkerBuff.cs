using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using LeagueSandbox.GameServer.API;

namespace Buffs
{
    class TalonShadowAssaultMarker : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IBuff ThisBuff;
        IMinion Blade;
        IParticle p;
        int previousIndicatorState;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            ThisBuff = buff;
            Blade = unit as IMinion;
            var ownerSkinID = Blade.Owner.SkinID;
            string particles;       
            unit.AddStatModifier(StatsModifier);
            buff.SetStatusEffect(StatusFlags.Targetable, false);
            buff.SetStatusEffect(StatusFlags.Ghosted, true);
			switch ((Blade.Owner as IObjAiBase).SkinID)
            {
                case 3:
                    particles = "talon_ult_blade_hold_dragon.troy";
                    break;

                case 4:
                    particles = "Talon_Skin04_ult_blade_hold.troy";
                    break;

                default:
                    particles = "Talon_Base_R_Blade_Hold.troy";
                    break;
            }
			ApiEventManager.OnSpellCast.AddListener(this, Blade.Owner.GetSpell("TalonRake"), WOnSpellCast);
			ApiEventManager.OnSpellCast.AddListener(this, Blade.Owner.GetSpell("TalonCutthroat"), EOnSpellPostCast);
			ApiEventManager.OnSpellCast.AddListener(this, Blade.Owner.GetSpell("TalonBasicAttack"), AOnSpellCast);
            ApiEventManager.OnSpellCast.AddListener(this, Blade.Owner.GetSpell("TalonShadowAssaultToggle"), R2OnSpellCast);
			var direction = new Vector3(Blade.Owner.Position.X, 0, Blade.Owner.Position.Y);
            p = AddParticle(Blade.Owner, null, particles, Blade.Position, buff.Duration,1f,"","",direction);
            //AddParticleTarget(Blade.Owner, Blade, particles, Blade, buff.Duration,1,"CHEST");
            //p = AddParticleTarget(Blade.Owner, Blade.Owner, "", Blade, buff.Duration, flags: FXFlags.TargetDirection);
        }
		public void WOnSpellCast(ISpell spell)
        {
            Blade.Owner.RemoveBuffsWithName("TalonShadowAssaultBuff");			
            ThisBuff.DeactivateBuff();
        }
		public void EOnSpellPostCast(ISpell spell)
        {
            Blade.Owner.RemoveBuffsWithName("TalonShadowAssaultBuff");			
            ThisBuff.DeactivateBuff();
        }
		public void AOnSpellCast(ISpell spell)
        {   
            Blade.Owner.RemoveBuffsWithName("TalonShadowAssaultBuff");		
            ThisBuff.DeactivateBuff();
        }
		public void R2OnSpellCast(ISpell spell)
        {   
            Blade.Owner.RemoveBuffsWithName("TalonShadowAssaultBuff");		
            ThisBuff.DeactivateBuff();
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (Blade != null && !Blade.IsDead)
            {
                if (p != null)
                {
                    p.SetToRemove();
                }
                Blade.Owner.RemoveBuffsWithName("TalonShadowAssaultToggle");
                SetStatus(Blade, StatusFlags.NoRender, true);
                AddParticle(Blade.Owner, null, "", Blade.Position);
				SpellCast(Blade.Owner, 4, SpellSlotType.ExtraSlots, true, Blade.Owner, Blade.Position);
                Blade.TakeDamage(Blade, 10000f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, DamageResultType.RESULT_NORMAL);
            }
        }
        public void OnUpdate(float diff)
        {          
        }
    }
}