using System;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;

namespace LeagueSandbox.GameServer.GameObjects.Stats
{
    public class Stats : IStats
    {
        public ulong SpellsEnabled { get; private set; }
        public ulong SummonerSpellsEnabled { get; private set; }

        public ActionState ActionState { get; private set; }
        public PrimaryAbilityResourceType ParType { get; private set; }

        public bool IsMagicImmune { get; set; }
        public bool IsInvulnerable { get; set; }
        public bool IsPhysicalImmune { get; set; }
        public bool IsLifestealImmune { get; set; }
        public bool IsTargetable { get; set; }
        public SpellDataFlags IsTargetableToTeam { get; set; }

        public float AttackSpeedFlat { get; set; }
        public float HealthPerLevel { get; set; }
        public float ManaPerLevel { get; set; }
        public float AdPerLevel { get; set; }
        public float ArmorPerLevel { get; set; }
        public float MagicResistPerLevel { get; set; }
        public float HealthRegenerationPerLevel { get; set; }
        public float ManaRegenerationPerLevel { get; set; }
        public float GrowthAttackSpeed { get; set; }
        public float[] ManaCost { get; }

        public IStat AbilityPower { get; }
        public IStat Armor { get; }
        public IStat ArmorPenetration { get; }
        public IStat AttackDamage { get; }
        public IStat AttackSpeedMultiplier { get; set; }
        public IStat CooldownReduction { get; }
        public IStat CriticalChance { get; }
        public IStat CriticalDamage { get; }
        public IStat GoldPerSecond { get; }
        public IStat HealthPoints { get; }
        public IStat HealthRegeneration { get; }
        public IStat LifeSteal { get; }
        public IStat MagicResist { get; }
        public IStat MagicPenetration { get; }
        public IStat ManaPoints { get; }
        public IStat ManaRegeneration { get; }
        public IStat MoveSpeed { get; }
        public IStat Range { get; }
        public IStat Size { get; }
        public IStat SpellVamp { get; }
        public IStat Tenacity { get; }

        public float Gold { get; set; }
        public byte Level { get; set; }
        public float Experience { get; set; }

        private float _currentHealth;
        public float CurrentHealth
        {
            get => Math.Min(HealthPoints.Total, _currentHealth);
            set => _currentHealth = value;
        }

        private float _currentMana;
        public float CurrentMana
        {
            get => Math.Min(ManaPoints.Total, _currentMana);
            set => _currentMana = value;
        }

        public bool IsGeneratingGold { get; set; } // Used to determine if the Stats update should include generating gold. Changed in Champion.h
        public float SpellCostReduction { get; set; } //URF Buff/Lissandra's passive

        public Stats()
        {
            SpellCostReduction = 0;
            ManaCost = new float[64];
            ActionState = ActionState.CAN_ATTACK | ActionState.CAN_CAST | ActionState.CAN_MOVE | ActionState.UNKNOWN;
            IsTargetable = true;
            IsTargetableToTeam = SpellDataFlags.TargetableToAll;

            AbilityPower = new Stat();
            Armor = new Stat();
            ArmorPenetration = new Stat();
            AttackDamage = new Stat();
            AttackSpeedMultiplier = new Stat(1.0f, 0, 0, 0, 0);
            CooldownReduction = new Stat();
            CriticalChance = new Stat();
            CriticalDamage = new Stat(2, 0, 0, 0, 0);
            GoldPerSecond = new Stat();
            HealthPoints = new Stat();
            HealthRegeneration = new Stat();
            LifeSteal = new Stat();
            MagicResist = new Stat();
            MagicPenetration = new Stat();
            ManaPoints = new Stat();
            ManaRegeneration = new Stat();
            MoveSpeed = new Stat();
            Range = new Stat();
            Size = new Stat(1.0f, 0, 0, 0, 0);
            SpellVamp = new Stat();
            Tenacity = new Stat();
        }

        public void LoadStats(ICharData charData)
        {
            HealthPoints.BaseValue = charData.BaseHp;
            ManaPoints.BaseValue = charData.BaseMp;
            AttackDamage.BaseValue = charData.BaseDamage;
            Range.BaseValue = charData.AttackRange;
            MoveSpeed.BaseValue = charData.MoveSpeed;
            Armor.BaseValue = charData.Armor;
            MagicResist.BaseValue = charData.SpellBlock;
            HealthRegeneration.BaseValue = charData.BaseStaticHpRegen;
            ManaRegeneration.BaseValue = charData.BaseStaticMpRegen;
            // AttackSpeedFlat = GlobalAttackSpeed / CharAttackDelay
            AttackSpeedFlat = (1.0f / charData.GlobalCharData.AttackDelay) / (1.0f + charData.AttackDelayOffsetPercent[0]);
            HealthPerLevel = charData.HpPerLevel;
            ManaPerLevel = charData.MpPerLevel;
            AdPerLevel = charData.DamagePerLevel;
            ArmorPerLevel = charData.ArmorPerLevel;
            MagicResistPerLevel = charData.SpellBlockPerLevel;
            HealthRegenerationPerLevel = charData.HpRegenPerLevel;
            ManaRegenerationPerLevel = charData.MpRegenPerLevel;
            GrowthAttackSpeed = charData.AttackSpeedPerLevel;
            ParType = charData.ParType;
        }

        public void AddModifier(IStatsModifier modifier)
        {
            AbilityPower.ApplyStatModifier(modifier.AbilityPower);
            Armor.ApplyStatModifier(modifier.Armor);
            ArmorPenetration.ApplyStatModifier(modifier.ArmorPenetration);
            AttackDamage.ApplyStatModifier(modifier.AttackDamage);
            AttackSpeedMultiplier.ApplyStatModifier(modifier.AttackSpeed);
            CriticalChance.ApplyStatModifier(modifier.CriticalChance);
            CriticalDamage.ApplyStatModifier(modifier.CriticalDamage);
            GoldPerSecond.ApplyStatModifier(modifier.GoldPerSecond);
            HealthPoints.ApplyStatModifier(modifier.HealthPoints);
            HealthRegeneration.ApplyStatModifier(modifier.HealthRegeneration);
            LifeSteal.ApplyStatModifier(modifier.LifeSteal);
            MagicResist.ApplyStatModifier(modifier.MagicResist);
            MagicPenetration.ApplyStatModifier(modifier.MagicPenetration);
            ManaPoints.ApplyStatModifier(modifier.ManaPoints);
            ManaRegeneration.ApplyStatModifier(modifier.ManaRegeneration);
            MoveSpeed.ApplyStatModifier(modifier.MoveSpeed);
            Range.ApplyStatModifier(modifier.Range);
            Size.ApplyStatModifier(modifier.Size);
            SpellVamp.ApplyStatModifier(modifier.SpellVamp);
            Tenacity.ApplyStatModifier(modifier.Tenacity);
        }

        public void RemoveModifier(IStatsModifier modifier)
        {
            AbilityPower.RemoveStatModifier(modifier.AbilityPower);
            Armor.RemoveStatModifier(modifier.Armor);
            ArmorPenetration.RemoveStatModifier(modifier.ArmorPenetration);
            AttackDamage.RemoveStatModifier(modifier.AttackDamage);
            AttackSpeedMultiplier.RemoveStatModifier(modifier.AttackSpeed);
            CriticalChance.RemoveStatModifier(modifier.CriticalChance);
            CriticalDamage.RemoveStatModifier(modifier.CriticalDamage);
            GoldPerSecond.RemoveStatModifier(modifier.GoldPerSecond);
            HealthPoints.RemoveStatModifier(modifier.HealthPoints);
            HealthRegeneration.RemoveStatModifier(modifier.HealthRegeneration);
            LifeSteal.RemoveStatModifier(modifier.LifeSteal);
            MagicResist.RemoveStatModifier(modifier.MagicResist);
            MagicPenetration.RemoveStatModifier(modifier.MagicPenetration);
            ManaPoints.RemoveStatModifier(modifier.ManaPoints);
            ManaRegeneration.RemoveStatModifier(modifier.ManaRegeneration);
            MoveSpeed.RemoveStatModifier(modifier.MoveSpeed);
            Range.RemoveStatModifier(modifier.Range);
            Size.RemoveStatModifier(modifier.Size);
            SpellVamp.RemoveStatModifier(modifier.SpellVamp);
            Tenacity.RemoveStatModifier(modifier.Tenacity);
        }

        public float GetTotalAttackSpeed()
        {
            return AttackSpeedFlat * AttackSpeedMultiplier.Total;
        }

        public void Update(float diff)
        {
            if (HealthRegeneration.Total > 0 && CurrentHealth < HealthPoints.Total && CurrentHealth > 0)
            {
                var newHealth = CurrentHealth + HealthRegeneration.Total * diff * 0.001f;
                newHealth = Math.Min(HealthPoints.Total, newHealth);
                CurrentHealth = newHealth;
            }

            if (IsGeneratingGold && GoldPerSecond.Total > 0)
            {
                var newGold = Gold + GoldPerSecond.Total * (diff * 0.001f);
                Gold = newGold;
            }

            if ((byte)ParType > 1)
            {
                return;
            }

            if (ManaRegeneration.Total > 0 && CurrentMana < ManaPoints.Total)
            {
                var newMana = CurrentMana + ManaRegeneration.Total * diff * 0.001f;
                newMana = Math.Min(ManaPoints.Total, newMana);
                CurrentMana = newMana;
            }
        }

        public void LevelUp()
        {
            Level++;

            StatsModifier statsLevelUp = new StatsModifier();
            statsLevelUp.HealthPoints.BaseValue = HealthPerLevel;
            statsLevelUp.ManaPoints.BaseValue = ManaPerLevel;
            statsLevelUp.AttackDamage.BaseValue = AdPerLevel;
            statsLevelUp.Armor.BaseValue = ArmorPerLevel;
            statsLevelUp.MagicResist.BaseValue = MagicResistPerLevel;
            statsLevelUp.HealthRegeneration.BaseValue = HealthRegenerationPerLevel;
            statsLevelUp.ManaRegeneration.BaseValue = ManaRegenerationPerLevel;
            if (Level > 1)
            {
                statsLevelUp.AttackSpeed.PercentBaseBonus = GrowthAttackSpeed / 100.0f;
            }
            AddModifier(statsLevelUp);

            CurrentHealth = HealthPoints.Total / (HealthPoints.Total - HealthPerLevel) * CurrentHealth;
            CurrentMana = ManaPoints.Total / (ManaPoints.Total - ManaPerLevel) * CurrentMana;
        }

        public bool GetSpellEnabled(byte id)
        {
            return (SpellsEnabled & 1u << id) != 0;
        }

        public void SetSpellEnabled(byte id, bool enabled)
        {
            if (enabled)
            {
                SpellsEnabled |= 1u << id;
            }
            else
            {
                SpellsEnabled &= ~(1u << id);
            }
        }

        public bool GetSummonerSpellEnabled(byte id)
        {
            return (SummonerSpellsEnabled & 16u << id) != 0;
        }

        public void SetSummonerSpellEnabled(byte id, bool enabled)
        {
            if (enabled)
            {
                SummonerSpellsEnabled |= 16u << id;
            }
            else
            {
                SummonerSpellsEnabled &= ~(16u << id);
            }
        }

        public bool GetActionState(ActionState state)
        {
            return ActionState.HasFlag(state);
        }

        public void SetActionState(ActionState state, bool enabled)
        {
            if (enabled)
            {
                ActionState |= state;
            }
            else
            {
                ActionState &= ~state;
            }
        }
    }
}
