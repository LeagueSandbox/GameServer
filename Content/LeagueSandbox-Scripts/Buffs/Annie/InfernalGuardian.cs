using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class InfernalGuardian : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IBuff b1;
        IBuff b2;
        IBuff thisBuff;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            thisBuff = buff;
            buff.SourceUnit.SetSpell("InfernalGuardianGuide", 3, true);
            b1 = AddBuff("InfernalGuardianTimer", buff.Duration, 1, ownerSpell, buff.SourceUnit, buff.SourceUnit);
            b2 = AddBuff("InfernalGuardianBurning", buff.Duration, 1, ownerSpell, unit, buff.SourceUnit);
            ApiEventManager.OnDeath.AddListener(this, unit, OnDeath, true);
        }

        public void OnDeath(IDeathData data)
        {
            b1.DeactivateBuff();
            b2.DeactivateBuff();
            thisBuff.DeactivateBuff();
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            unit.Die(CreateDeathData(false, 0, unit, unit, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, 0.0f));
            buff.SourceUnit.SetSpell("InfernalGuardian", 3, true);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}