using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain;
using GameServerLib.GameObjects.AttackableUnits;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Passives
{
    public class TalonMercy : ICharScript
    {
        ISpell Spell;
		IAttackableUnit Target;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            Spell = spell;
            {
                ApiEventManager.OnHitUnit.AddListener(this, owner, OnHitUnit, false);
            }
        }
        public void OnHitUnit(IDamageData data)        
        {
            var owner = Spell.CastInfo.Owner;
			var damage = owner.Stats.AttackDamage.Total * 0.1f ;
			var ELevel = owner.GetSpell("TalonCutthroat").CastInfo.SpellLevel;
			var damageamp = 0.03f * (ELevel - 1);
			var Edamage = owner.Stats.AttackDamage.Total * damageamp ;
			if (Target.HasBuff("TalonSlow")||Target.HasBuff("TalonESlow")||Target.HasBuff("Stun")||Target.HasBuff("Slow")|| Target.HasBuff("Disarm")|| Target.HasBuff("Silence")|| Target.HasBuff("Blind")|| Target.HasBuff("Pulverize"))
			{
				Target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_PERIODIC, false);
			}
			if (Target.HasBuff("TalonDamageAmp"))
            {
				Target.TakeDamage(owner, Edamage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_PERIODIC, false);
            }
        }       
        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
            ApiEventManager.OnHitUnit.RemoveListener(this);
        }
        public void OnUpdate(float diff)
        {
        }
    }
}

