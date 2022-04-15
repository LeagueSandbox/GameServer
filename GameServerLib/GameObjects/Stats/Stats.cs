using System;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

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
        public IStat AttackDamagePerLevel { get; set; }
        public IStat AttackSpeedMultiplier { get; set; }
        public IStat CooldownReduction { get; }
        public IStat CriticalChance { get; }
        public IStat CriticalDamage { get; }
        public IStat ExpGivenOnDeath { get; }
        public IStat GoldPerGoldTick { get; }
        public IStat GoldGivenOnDeath { get; }
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
        public IStat AcquisitionRange { get; set; }

        public float Gold { get; set; }
        public byte Level { get; set; }
        public float Experience { get; set; }
        public float Points { get; set; }

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

        public void LoadStats(ICharData charData)
        {
            AcquisitionRange.BaseValue = charData.AcquisitionRange;
            AttackDamagePerLevel.BaseValue = charData.DamagePerLevel;
            Armor.BaseValue = charData.Armor;
            ArmorPerLevel = charData.ArmorPerLevel;
            AttackDamage.BaseValue = charData.BaseDamage;
            // AttackSpeedFlat = GlobalAttackSpeed / CharAttackDelay
            AttackSpeedFlat = (1.0f / charData.GlobalCharData.AttackDelay) / (1.0f + charData.AttackDelayOffsetPercent[0]);
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
        }

        public void AddModifier(IStatsModifier modifier)
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
            statsLevelUp.AttackDamage.BaseValue = AttackDamagePerLevel.Total;
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

        public float GetPostMitigationDamage(float damage, DamageType type, IAttackableUnit attacker)
        {
            float defense = 0;
            switch (type)
            {
                case DamageType.DAMAGE_TYPE_PHYSICAL:
                    defense = Armor.Total;
                    defense = (1 - attacker.Stats.ArmorPenetration.PercentBonus) * defense -
                              attacker.Stats.ArmorPenetration.FlatBonus;

                    break;
                case DamageType.DAMAGE_TYPE_MAGICAL:
                    defense = MagicResist.Total;
                    defense = (1 - attacker.Stats.MagicPenetration.PercentBonus) * defense -
                              attacker.Stats.MagicPenetration.FlatBonus;
                    break;
                case DamageType.DAMAGE_TYPE_TRUE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            if (damage < 0f)
            {
                damage = 0f;
            }
            damage = defense >= 0 ? 100 / (100 + defense) * damage : (2 - 100 / (100 - defense)) * damage;
            return damage;
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
