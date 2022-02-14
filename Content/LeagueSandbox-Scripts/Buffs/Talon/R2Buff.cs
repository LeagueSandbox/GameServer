using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    public class TalonShadowAssaultBuff : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();
        IBuff ThisBuff;
        IParticle p;
		IParticle p2;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
			//ApiEventManager.OnSpellCast.AddListener(this, ownerSpell.CastInfo.Owner.GetSpell("TalonRake"), WOnSpellCast);
			//ApiEventManager.OnSpellCast.AddListener(this, ownerSpell.CastInfo.Owner.GetSpell("TalonCutthroat"), EOnSpellPostCast);
			//ApiEventManager.OnSpellCast.AddListener(this, ownerSpell.CastInfo.Owner.GetSpell("TalonBasicAttack"), AOnSpellCast);
			ApiEventManager.OnSpellCast.AddListener(this, ownerSpell.CastInfo.Owner.GetSpell("TalonShadowAssaultToggle"), R2OnSpellCast);
            StatsModifier.MoveSpeed.PercentBonus += 0.4f;
            unit.AddStatModifier(StatsModifier);
            p = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "talon_invis_cas.troy", unit, 2.5f);
			p2 = AddParticleTarget(ownerSpell.CastInfo.Owner, ownerSpell.CastInfo.Owner, "talon_ult_sound.troy", ownerSpell.CastInfo.Owner, 10f);
            if (unit is IObjAiBase owner)
            {
          
                var r2Spell = owner.SetSpell("TalonShadowAssaultToggle", 3, true);
				//CreateTimer((float) 0.5f , () =>
                //{
				//ownerSpell.CastInfo.Owner.GetSpell("TalonShadowAssaultToggle").SetCooldown(0f);
				//});
            }
        }
		
		public void R2OnSpellCast(ISpell spell)
        {          
            ThisBuff.DeactivateBuff();
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {   
             RemoveParticle(p2);
             (unit as IObjAiBase).SetSpell("TalonShadowAssault", 3, true);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}