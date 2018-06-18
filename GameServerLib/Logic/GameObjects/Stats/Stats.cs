using LeagueSandbox.GameServer.Logic.Content;
using System;
using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Stats
    {
        protected UInt32 _spellsEnabled;
        protected UInt32 _summonerSpellsEnabled;

        public float AttackSpeedFlat { get; set; }
        public float HealthPerLevel { get; set; }
        public float ManaPerLevel { get; set; }
        public float AdPerLevel { get; set; }
        public float ArmorPerLevel { get; set; }
        public float MagicResistPerLevel { get; set; }
        public float HealthRegenerationPerLevel { get; set; }
        public float ManaRegenerationPerLevel { get; set; }
        public float GrowthAttackSpeed { get; set; }
        public float[] ManaCost { get; set; }

        public Stat AbilityPower { get; }
        public Stat Armor { get; }
        public Stat ArmorPenetration { get; }
        public Stat AttackDamage { get; }
        public Stat AttackSpeedMultiplier { get; }
        public Stat CooldownReduction { get; }
        public Stat CriticalChance { get; }
        public Stat GoldPerSecond { get; }
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

        public float Gold { get; set; }

        public byte Level { get; set; }

        public float Experience { get; set; }

        public float CurrentHealth { get; set; }

        public float CurrentMana { get; set; }

        protected bool generatingGold; // Used to determine if the Stats update should include generating gold. Changed in Champion.h
        protected float spellCostReduction; //URF Buff/Lissandra's passive
        protected float critDamagePct; //Default = 2... add with runes/items (change with yasuo's passive)

        public Stats()
        {
            spellCostReduction = 0;
            critDamagePct = 2;
            ManaCost = new float[64];

            AbilityPower = new Stat();
            Armor = new Stat();
            ArmorPenetration = new Stat();
            AttackDamage = new Stat();
            AttackSpeedMultiplier = new Stat(1.0f, 0, 0, 0, 0);
            CooldownReduction = new Stat();
            CriticalChance = new Stat();
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
        public void LoadStats(CharData charData)
        {
            HealthPoints.BaseValue = charData.BaseHP;
            ManaPoints.BaseValue = charData.BaseMP;
            AttackDamage.BaseValue = charData.BaseDamage;
            Range.BaseValue = charData.AttackRange;
            MoveSpeed.BaseValue = charData.MoveSpeed;
            Armor.BaseValue = charData.Armor;
            MagicResist.BaseValue = charData.SpellBlock;
            HealthRegeneration.BaseValue = charData.BaseStaticHPRegen;
            ManaRegeneration.BaseValue = charData.BaseStaticMPRegen;
            AttackSpeedFlat = 0.625f / (1 + charData.AttackDelayOffsetPercent);
            HealthPerLevel = charData.HPPerLevel;
            ManaPerLevel = charData.MPPerLevel;
            AdPerLevel = charData.DamagePerLevel;
            ArmorPerLevel = charData.ArmorPerLevel;
            MagicResistPerLevel = charData.SpellBlockPerLevel;
            HealthRegenerationPerLevel = charData.HPRegenPerLevel;
            ManaRegenerationPerLevel = charData.MPRegenPerLevel;
            GrowthAttackSpeed = charData.AttackSpeedPerLevel;
        }

        public void UpdateModifier(IStatsModifier modifier)
        {
            RemoveModifier(modifier);
            AddModifier(modifier);
        }

        public void AddModifier(IStatsModifier modifier)
        {
            AbilityPower.ApplyStatModificator(modifier.AbilityPower);
            Armor.ApplyStatModificator(modifier.Armor);
            ArmorPenetration.ApplyStatModificator(modifier.ArmorPenetration);
            AttackDamage.ApplyStatModificator(modifier.AttackDamage);
            AttackSpeedMultiplier.ApplyStatModificator(modifier.AttackSpeed);
            CriticalChance.ApplyStatModificator(modifier.CriticalChance);
            GoldPerSecond.ApplyStatModificator(modifier.GoldPerSecond);
            HealthPoints.ApplyStatModificator(modifier.HealthPoints);
            HealthRegeneration.ApplyStatModificator(modifier.HealthRegeneration);
            LifeSteal.ApplyStatModificator(modifier.LifeSteel);
            MagicResist.ApplyStatModificator(modifier.MagicResist);
            MagicPenetration.ApplyStatModificator(modifier.MagicPenetration);
            ManaPoints.ApplyStatModificator(modifier.ManaPoints);
            ManaRegeneration.ApplyStatModificator(modifier.ManaRegeneration);
            MoveSpeed.ApplyStatModificator(modifier.MoveSpeed);
            Range.ApplyStatModificator(modifier.Range);
            Size.ApplyStatModificator(modifier.Size);
            SpellVamp.ApplyStatModificator(modifier.SpellVamp);
            Tenacity.ApplyStatModificator(modifier.Tenacity);
        }

        public void RemoveModifier(IStatsModifier modifier)
        {
            AbilityPower.RemoveStatModificator(modifier.AbilityPower);
            Armor.RemoveStatModificator(modifier.Armor);
            ArmorPenetration.RemoveStatModificator(modifier.ArmorPenetration);
            AttackDamage.RemoveStatModificator(modifier.AttackDamage);
            AttackSpeedMultiplier.RemoveStatModificator(modifier.AttackSpeed);
            CriticalChance.RemoveStatModificator(modifier.CriticalChance);
            GoldPerSecond.RemoveStatModificator(modifier.GoldPerSecond);
            HealthPoints.RemoveStatModificator(modifier.HealthPoints);
            HealthRegeneration.RemoveStatModificator(modifier.HealthRegeneration);
            LifeSteal.RemoveStatModificator(modifier.LifeSteel);
            MagicResist.RemoveStatModificator(modifier.MagicResist);
            MagicPenetration.RemoveStatModificator(modifier.MagicPenetration);
            ManaPoints.RemoveStatModificator(modifier.ManaPoints);
            ManaRegeneration.RemoveStatModificator(modifier.ManaRegeneration);
            MoveSpeed.RemoveStatModificator(modifier.MoveSpeed);
            Range.RemoveStatModificator(modifier.Range);
            Size.RemoveStatModificator(modifier.Size);
            SpellVamp.RemoveStatModificator(modifier.SpellVamp);
            Tenacity.RemoveStatModificator(modifier.Tenacity);
        }


        public bool IsGeneratingGold()
        {
            return generatingGold;
        }

        public void SetGeneratingGold(bool b)
        {
            generatingGold = b;
        }

        public float GetTotalAttackSpeed()
        {
            return AttackSpeedFlat*AttackSpeedMultiplier.Total;
        }

        public void update(float diff)
        {
            if (HealthRegeneration.Total > 0 && CurrentHealth < HealthPoints.Total && CurrentHealth > 0)
            {
                var newHealth = CurrentHealth + HealthRegeneration.Total * diff * 0.001f;
                newHealth = Math.Min(HealthPoints.Total, newHealth);
                CurrentHealth = newHealth;
            }

            if (ManaRegeneration.Total > 0 && CurrentMana < ManaPoints.Total)
            {
                var newMana = CurrentMana + (ManaRegeneration.Total * diff * 0.001f);
                newMana = Math.Min(ManaPoints.Total, newMana);
                CurrentMana = newMana;
            }
            if (generatingGold && GoldPerSecond.Total > 0)
            {
                var newGold = Gold + GoldPerSecond.Total * (diff * 0.001f);
                Gold = newGold;
            }
        }

        public void LevelUp()
        {
            Level++;

            HealthPoints.BaseValue += HealthPerLevel;
            CurrentHealth = (HealthPoints.Total / (HealthPoints.Total - HealthPerLevel)) * CurrentHealth;
            ManaPoints.BaseValue = ManaPoints.Total + ManaPerLevel;
            CurrentMana = (ManaPoints.Total / (ManaPoints.Total - ManaPerLevel)) * CurrentMana;
            AttackDamage.BaseValue = AttackDamage.BaseValue + AdPerLevel;
            Armor.BaseValue = Armor.BaseValue + ArmorPerLevel;
            MagicResist.BaseValue = MagicResist.Total + MagicResistPerLevel;
            HealthRegeneration.BaseValue = HealthRegeneration.BaseValue + HealthRegenerationPerLevel;
            ManaRegeneration.BaseValue = ManaRegeneration.BaseValue + ManaRegenerationPerLevel;
        }

        public float GetLevel()
        {
            return Level;
        }

        public float getSpellCostReduction()
        {
            return spellCostReduction;
        }

        public void setSpellCostReduction(float scr)
        {
            spellCostReduction = scr;
        }

        public float getCritDamagePct()
        {
            return critDamagePct;
        }

        public void setCritDamagePct(float critDmg)
        {
            critDamagePct = critDmg;
        }

        public bool getSpellEnabled(byte id)
        {
            return (_spellsEnabled & (1 << id)) != 0;
        }

        public void setSpellEnabled(byte id, bool enabled)
        {
            if (enabled)
                _spellsEnabled |= (UInt32)(1 << id);
            else
                _spellsEnabled &= (UInt32)(~(1 << id));
        }

        public bool getSummonerSpellEnabled(byte id)
        {
            return (_summonerSpellsEnabled & (1 << id)) != 0;
        }

        public void setSummonerSpellEnabled(byte id, bool enabled)
        { 
            if (enabled)
                _summonerSpellsEnabled |= (UInt32)(16 << id);
            else
                _summonerSpellsEnabled &= (UInt32)(~(16 << id));
        }
    }
}
