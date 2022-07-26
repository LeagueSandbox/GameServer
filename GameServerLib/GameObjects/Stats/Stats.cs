using System;
using System.Collections.Generic;
using System.Linq;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.GameObjects.StatsNS
{
    public class Stats
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
        public float ArmorPerLevel { get; set; }
        public float MagicResistPerLevel { get; set; }
        public float HealthRegenerationPerLevel { get; set; }
        public float ManaRegenerationPerLevel { get; set; }
        public float GrowthAttackSpeed { get; set; }
        public float[] ManaCost { get; }

        public Stat AbilityPower { get; }
        public Stat Armor { get; }
        public Stat ArmorPenetration { get; }
        public Stat AttackDamage { get; }
        public Stat AttackDamagePerLevel { get; set; }
        public Stat AttackSpeedMultiplier { get; set; }
        public Stat CooldownReduction { get; }
        public Stat CriticalChance { get; }
        public Stat CriticalDamage { get; }
        public Stat ExpGivenOnDeath { get; }
        public Stat GoldPerGoldTick { get; }
        public Stat GoldGivenOnDeath { get; }
        public Stat HealthPoints { get; }
        public Stat HealthRegeneration { get; }
        public Stat LifeSteal { get; }
        public Stat MagicResist { get; }
        public Stat MagicPenetration { get; }
        public Stat ManaPoints { get; }
        public Stat ManaRegeneration { get; }
        public Stat MoveSpeed { get; }
        public Stat Range { get; }
        public Stat Size { get; }
        public Stat SpellVamp { get; }
        public Stat Tenacity { get; }
        public Stat AcquisitionRange { get; set; }

        public float Gold { get; set; }
        public byte Level { get; set; }
        public float Experience { get; set; }
        public float Points { get; set; }
        public float SlowResistPercent { get; set; }
        public float MultiplicativeSpeedBonus { get; set; }
        private List<float> _slows = new List<float>();
        private float _currentHealth;
        private float _trueMoveSpeed;
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
            Level = 1;
            SpellCostReduction = 0;
            ManaCost = new float[64];
            ActionState = ActionState.CAN_ATTACK | ActionState.CAN_CAST | ActionState.CAN_MOVE | ActionState.TARGETABLE;
            IsTargetable = true;
            IsTargetableToTeam = SpellDataFlags.TargetableToAll;

            AbilityPower = new Stat();
            Armor = new Stat();
            ArmorPenetration = new Stat();
            AttackDamage = new Stat();
            AttackDamagePerLevel = new Stat();
            AttackSpeedMultiplier = new Stat(1.0f, 0, 0, 0, 0);
            CooldownReduction = new Stat();
            CriticalChance = new Stat();
            CriticalDamage = new Stat();
            ExpGivenOnDeath = new Stat();
            GoldPerGoldTick = new Stat();
            GoldGivenOnDeath = new Stat();
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
            AcquisitionRange = new Stat();
        }

        public void LoadStats(CharData charData)
        {
            AcquisitionRange.BaseValue = charData.AcquisitionRange;
            AttackDamagePerLevel.BaseValue = charData.DamagePerLevel;
            Armor.BaseValue = charData.Armor;
            ArmorPerLevel = charData.ArmorPerLevel;
            AttackDamage.BaseValue = charData.BaseDamage;
            // AttackSpeedFlat = GlobalAttackSpeed / CharAttackDelay
            AttackSpeedFlat = 1.0f / GlobalData.GlobalCharacterDataConstants.AttackDelay / (1.0f + charData.AttackDelayOffsetPercent);
            CriticalDamage.BaseValue = charData.CritDamageBonus;
            ExpGivenOnDeath.BaseValue = charData.ExpGivenOnDeath;
            GoldGivenOnDeath.BaseValue = charData.GoldGivenOnDeath;
            GrowthAttackSpeed = charData.AttackSpeedPerLevel;
            HealthPerLevel = charData.HpPerLevel;
            HealthPoints.BaseValue = charData.BaseHp;
            HealthRegeneration.BaseValue = charData.BaseStaticHpRegen;
            HealthRegenerationPerLevel = charData.HpRegenPerLevel;
            MagicResist.BaseValue = charData.SpellBlock;
            MagicResistPerLevel = charData.SpellBlockPerLevel;
            ManaPerLevel = charData.MpPerLevel;
            ManaPoints.BaseValue = charData.BaseMp;
            ManaRegeneration.BaseValue = charData.BaseStaticMpRegen;
            ManaRegenerationPerLevel = charData.MpRegenPerLevel;
            MoveSpeed.BaseValue = charData.MoveSpeed;
            ParType = charData.ParType;
            Range.BaseValue = charData.AttackRange;
            CalculateTrueMoveSpeed();
        }

        public void AddModifier(StatsModifier modifier)
        {
            AbilityPower.ApplyStatModifier(modifier.AbilityPower);
            Armor.ApplyStatModifier(modifier.Armor);
            ArmorPenetration.ApplyStatModifier(modifier.ArmorPenetration);
            AttackDamage.ApplyStatModifier(modifier.AttackDamage);
            AttackDamagePerLevel.ApplyStatModifier(modifier.AttackDamagePerLevel);
            AttackSpeedMultiplier.ApplyStatModifier(modifier.AttackSpeed);
            CooldownReduction.ApplyStatModifier(modifier.CooldownReduction);
            CriticalChance.ApplyStatModifier(modifier.CriticalChance);
            CriticalDamage.ApplyStatModifier(modifier.CriticalDamage);
            GoldPerGoldTick.ApplyStatModifier(modifier.GoldPerSecond);
            HealthPoints.ApplyStatModifier(modifier.HealthPoints);
            HealthRegeneration.ApplyStatModifier(modifier.HealthRegeneration);
            LifeSteal.ApplyStatModifier(modifier.LifeSteal);
            MagicResist.ApplyStatModifier(modifier.MagicResist);
            MagicPenetration.ApplyStatModifier(modifier.MagicPenetration);
            ManaPoints.ApplyStatModifier(modifier.ManaPoints);
            ManaRegeneration.ApplyStatModifier(modifier.ManaRegeneration);

            if (modifier.MoveSpeed.PercentBonus < 0)
            {
                _slows.Add(modifier.MoveSpeed.PercentBonus);
            }
            else
            {
                MoveSpeed.ApplyStatModifier(modifier.MoveSpeed);
            }
            CalculateTrueMoveSpeed();

            Range.ApplyStatModifier(modifier.Range);
            Size.ApplyStatModifier(modifier.Size);
            SpellVamp.ApplyStatModifier(modifier.SpellVamp);
            Tenacity.ApplyStatModifier(modifier.Tenacity);
            SlowResistPercent += modifier.SlowResistPercent;
            MultiplicativeSpeedBonus += modifier.MultiplicativeSpeedBonus;
        }

        public void RemoveModifier(StatsModifier modifier)
        {
            AbilityPower.RemoveStatModifier(modifier.AbilityPower);
            Armor.RemoveStatModifier(modifier.Armor);
            ArmorPenetration.RemoveStatModifier(modifier.ArmorPenetration);
            AttackDamage.RemoveStatModifier(modifier.AttackDamage);
            AttackSpeedMultiplier.RemoveStatModifier(modifier.AttackSpeed);
            CooldownReduction.RemoveStatModifier(modifier.CooldownReduction);
            CriticalChance.RemoveStatModifier(modifier.CriticalChance);
            CriticalDamage.RemoveStatModifier(modifier.CriticalDamage);
            GoldPerGoldTick.RemoveStatModifier(modifier.GoldPerSecond);
            HealthPoints.RemoveStatModifier(modifier.HealthPoints);
            HealthRegeneration.RemoveStatModifier(modifier.HealthRegeneration);
            LifeSteal.RemoveStatModifier(modifier.LifeSteal);
            MagicResist.RemoveStatModifier(modifier.MagicResist);
            MagicPenetration.RemoveStatModifier(modifier.MagicPenetration);
            ManaPoints.RemoveStatModifier(modifier.ManaPoints);
            ManaRegeneration.RemoveStatModifier(modifier.ManaRegeneration);

            if (modifier.MoveSpeed.PercentBonus < 0)
            {
                _slows.Remove(modifier.MoveSpeed.PercentBonus);
            }
            else
            {
                MoveSpeed.RemoveStatModifier(modifier.MoveSpeed);
            }
            CalculateTrueMoveSpeed();

            Range.RemoveStatModifier(modifier.Range);
            Size.RemoveStatModifier(modifier.Size);
            SpellVamp.RemoveStatModifier(modifier.SpellVamp);
            Tenacity.RemoveStatModifier(modifier.Tenacity);
            SlowResistPercent -= modifier.SlowResistPercent;
            MultiplicativeSpeedBonus -= modifier.MultiplicativeSpeedBonus;
        }

        public float GetTotalAttackSpeed()
        {
            return AttackSpeedFlat * AttackSpeedMultiplier.Total;
        }

        public float GetTrueMoveSpeed()
        {
            return _trueMoveSpeed;
        }

        public void ClearSlows()
        {
            _slows.Clear();
            CalculateTrueMoveSpeed();
        }

        public void Update(float diff)
        {
            if (HealthRegeneration.Total > 0 && CurrentHealth < HealthPoints.Total && CurrentHealth > 0)
            {
                var newHealth = CurrentHealth + HealthRegeneration.Total * diff * 0.001f;
                newHealth = Math.Min(HealthPoints.Total, newHealth);
                CurrentHealth = newHealth;
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
            statsLevelUp.HealthPoints.BaseValue = GetLevelUpStatValue(HealthPerLevel);
            statsLevelUp.ManaPoints.BaseValue = GetLevelUpStatValue(ManaPerLevel);
            statsLevelUp.AttackDamage.BaseValue = GetLevelUpStatValue(AttackDamagePerLevel.BaseValue);
            statsLevelUp.AttackDamage.FlatBonus = GetLevelUpStatValue(AttackDamagePerLevel.FlatBonus);
            statsLevelUp.Armor.BaseValue = GetLevelUpStatValue(ArmorPerLevel);
            statsLevelUp.MagicResist.BaseValue = GetLevelUpStatValue(MagicResistPerLevel);
            statsLevelUp.HealthRegeneration.BaseValue = GetLevelUpStatValue(HealthRegenerationPerLevel);
            statsLevelUp.ManaRegeneration.BaseValue = GetLevelUpStatValue(ManaRegenerationPerLevel);
            statsLevelUp.AttackSpeed.PercentBaseBonus = GetLevelUpStatValue(GrowthAttackSpeed / 100.0f);
            AddModifier(statsLevelUp);

            CurrentHealth += statsLevelUp.HealthPoints.BaseValue;
            CurrentMana += statsLevelUp.ManaPoints.BaseValue;
        }

        public float GetLevelUpStatValue(float value)
        {
            return value * (0.65f + 0.035f * Level);
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

        public float GetPostMitigationDamage(float damage, DamageType type, AttackableUnit attacker)
        {
            if (damage <= 0f)
            {
                return 0.0f;
            }

            float stat;
            switch (type)
            {
                case DamageType.DAMAGE_TYPE_PHYSICAL:
                    stat = Armor.Total;
                    break;
                case DamageType.DAMAGE_TYPE_MAGICAL:
                    stat = MagicResist.Total;
                    break;
                case DamageType.DAMAGE_TYPE_TRUE:
                    return damage;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            float mitigationPercent = 100 / (100 + stat);

            if (stat < 0)
            {
                mitigationPercent = 2 - mitigationPercent;
            }

            return damage * mitigationPercent;
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

        public void CalculateTrueMoveSpeed()
        {
            float speed = MoveSpeed.BaseValue + MoveSpeed.FlatBonus;
            if (speed > 490.0f)
            {
                speed = speed * 0.5f + 230.0f;
            }
            else if (speed >= 415.0f)
            {
                speed = speed * 0.8f + 83.0f;
            }
            else if (speed < 220.0f)
            {
                speed = speed * 0.5f + 110.0f;
            }

            speed = speed * (1 + MoveSpeed.PercentBonus) * (1 + MultiplicativeSpeedBonus);

            if (_slows.Count > 0)
            {
                //Only takes into account the highest slow
                speed *= 1 + _slows.Max(z => z) * (1 - SlowResistPercent);
            }

            _trueMoveSpeed = speed;
        }
    }
}
