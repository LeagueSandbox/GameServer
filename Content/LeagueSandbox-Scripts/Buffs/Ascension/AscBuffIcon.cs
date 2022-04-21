using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.API;

namespace Buffs
{
    internal class AscBuffIcon : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };
        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();
        IStatsModifier LevelUpModifier = new StatsModifier();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            //TODO: Add 100% mana/energy cost reduction and 50% Health cost reduction
            StatsModifier.HealthPoints.FlatBonus = 50.0f * unit.Stats.Level;
            StatsModifier.AttackDamage.FlatBonus = 12.0f * unit.Stats.Level;
            StatsModifier.AbilityPower.FlatBonus = 12.0f * unit.Stats.Level;
            StatsModifier.ArmorPenetration.PercentBonus = 0.15f;
            StatsModifier.MagicPenetration.PercentBonus = 0.15f;
            StatsModifier.CooldownReduction.FlatBonus = 0.25f;
            StatsModifier.Size.FlatBonus = 0.5f;
            unit.AddStatModifier(StatsModifier);
            unit.Stats.CurrentHealth = unit.Stats.HealthPoints.Total;

            LevelUpModifier.HealthPoints.FlatBonus = 50.0f;
            LevelUpModifier.AttackDamage.FlatBonus = 12.0f;
            LevelUpModifier.AbilityPower.FlatBonus = 12.0f;

            ApiEventManager.OnLevelUp.AddListener(this, unit, OnLevelUp, false);

            //TODO: Add buff tooltip updates when we find out why they can be updated ATM.
        }

        public void OnLevelUp(IAttackableUnit unit)
        {
            StatsModifier.HealthPoints.FlatBonus += 50.0f;
            StatsModifier.AttackDamage.FlatBonus += 12.0f;
            StatsModifier.AbilityPower.FlatBonus += 12.0f;
            unit.Stats.CurrentHealth += LevelUpModifier.HealthPoints.FlatBonus;

            unit.AddStatModifier(LevelUpModifier);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}