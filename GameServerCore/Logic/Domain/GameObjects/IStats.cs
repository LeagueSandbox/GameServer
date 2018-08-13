using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServerCore.Logic.Enums;

namespace GameServerCore.Logic.Domain.GameObjects
{
    public interface IStats
    {
        ulong SpellsEnabled { get; }
        ulong SummonerSpellsEnabled { get; }
        ActionState ActionState { get; }
        PrimaryAbilityResourceType ParType { get; }
        bool IsMagicImmune { get; }
        bool IsInvulnerable { get; }
        bool IsPhysicalImmune { get; }
        bool IsLifestealImmune { get; }
        bool IsTargetable { get; }
        IsTargetableToTeamFlags IsTargetableToTeam { get; }
        float AttackSpeedFlat { get; }
        float HealthPerLevel { get; }
        float ManaPerLevel { get; }
        float AdPerLevel { get; }
        float ArmorPerLevel { get; }
        float MagicResistPerLevel { get; }
        float HealthRegenerationPerLevel { get; }
        float ManaRegenerationPerLevel { get; }
        float GrowthAttackSpeed { get; }
        float[] ManaCost { get; }
        IStat AbilityPower { get; }
        IStat Armor { get; }
        IStat ArmorPenetration { get; }
        IStat AttackDamage { get; }
        IStat AttackSpeedMultiplier { get; }
        IStat CooldownReduction { get; }
        IStat CriticalChance { get; }
        IStat CriticalDamage { get; }
        IStat GoldPerSecond { get; }
        IStat HealthPoints { get; }
        IStat HealthRegeneration { get; }
        IStat LifeSteal { get; }
        IStat MagicResist { get; }
        IStat MagicPenetration { get; }
        IStat ManaPoints { get; }
        IStat ManaRegeneration { get; }
        IStat MoveSpeed { get; }
        IStat Range { get; }
        IStat Size { get; }
        IStat SpellVamp { get; }
        IStat Tenacity { get; }
        float Gold { get; set; }
        byte Level { get; set; }
        float Experience { get; set; }
        float CurrentHealth { get; set; }
        float CurrentMana { get; set; }
        bool IsGeneratingGold { get; }
        float SpellCostReduction { get; }

        bool GetSpellEnabled(byte id);
        void SetSpellEnabled(byte id, bool enabled);
        bool GetSummonerSpellEnabled(byte id);
        void SetSummonerSpellEnabled(byte id, bool enabled);
        void AddModifier(IStatsModifier modifier);
        void RemoveModifier(IStatsModifier modifier);
    }
}
