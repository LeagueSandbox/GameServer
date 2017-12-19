using LeagueSandbox.GameServer.Logic.Content;
using System;
using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public enum FieldMask : uint
    {
        FM1_Gold = 0x00000001,
        FM1_Gold_Total = 0x00000002,
        FM1_Spells_Enabled = 0x00000004, // Lower bits. Bits: 0-3 -> Q-R, 4-9 -> Items, 10 -> Trinket
        FM1_Spells_Enabled2 = 0x00000008, // Upper bits
        FM1_SummonerSpells_Enabled = 0x00000010, // Lower bits. Bits: 0 -> D, 1 -> F
        FM1_SummonerSpells_Enabled2 = 0x00000020, // Upper bits
        FM1_EvolvePoints = 0x00000040,
        FM1_EvolveFlags = 0x00000080,
        FM1_ManaCost0 = 0x00000100,
        FM1_ManaCost1 = 0x00000200,
        FM1_ManaCost2 = 0x00000400,
        FM1_ManaCost3 = 0x00000800,
        FM1_ManaCostEx0 = 0x00001000,
        FM1_ManaCostEx1 = 0x00002000,
        FM1_ManaCostEx2 = 0x00004000,
        FM1_ManaCostEx3 = 0x00008000,
        FM1_ManaCostEx4 = 0x00010000,
        FM1_ManaCostEx5 = 0x00020000,
        FM1_ManaCostEx6 = 0x00040000,
        FM1_ManaCostEx7 = 0x00080000,
        FM1_ManaCostEx8 = 0x00100000,
        FM1_ManaCostEx9 = 0x00200000,
        FM1_ManaCostEx10 = 0x00400000,
        FM1_ManaCostEx11 = 0x00800000,
        FM1_ManaCostEx12 = 0x01000000,
        FM1_ManaCostEx13 = 0x02000000,
        FM1_ManaCostEx14 = 0x04000000,
        FM1_ManaCostEx15 = 0x08000000,

        FM2_Base_Ad = 0x00000020, // champ's base ad that increase every level. No item bonus should be added here
        FM2_Base_Ap = 0x00000040,
        FM2_Crit_Chance = 0x00000100, // 0.5 = 50%
        FM2_Armor = 0x00000200,
        FM2_Magic_Armor = 0x00000400,
        FM2_Hp5 = 0x00000800,
        FM2_Mp5 = 0x00001000,
        FM2_Range = 0x00002000,
        FM2_Bonus_Ad_Flat = 0x00004000, // AD flat bonuses
        FM2_Bonus_Ad_Pct = 0x00008000, // AD percentage bonuses. 0.5 = 50%
        FM2_Bonus_Ap_Flat = 0x00010000, // AP flat bonuses
        FM2_Bonus_Ap_Pct = 0x00020000, // AP flat bonuses
        FM2_Atks_multiplier = 0x00080000, // Attack speed multiplier. If set to 2 and champ's base attack speed is 0.600, then his new AtkSpeed becomes 1.200
        FM2_cdr = 0x00400000, // Cooldown reduction. 0.5 = 50%
        FM2_Armor_Pen_Flat = 0x01000000,
        FM2_Armor_Pen_Pct = 0x02000000, // Armor pen. 0.5 = 50%
        FM2_Magic_Pen_Flat = 0x04000000,
        FM2_Magic_Pen_Pct = 0x08000000,
        FM2_LifeSteal = 0x10000000, //Life Steal. 0.5 = 50%
        FM2_SpellVamp = 0x20000000, //Spell Vamp. 0.5 = 50%
        FM2_Tenacity = 0x40000000, //Tenacity. 0.5 = 50%

        FM3_Armor_Pen_Pct = 0x00000001, //Armor pen. 1.7 = 30% -- These are probably consequence of some bit ops
        FM3_Magic_Pen_Pct = 0x00000002, //Magic pen. 1.7 = 30%

        FM4_CurrentHp = 0x00000001,
        FM4_CurrentMana = 0x00000002,
        FM4_MaxHp = 0x00000004,
        FM4_MaxMp = 0x00000008,
        FM4_exp = 0x00000010,
        FM4_Vision1 = 0x00000100,
        FM4_Vision2 = 0x00000200,
        FM4_Speed = 0x00000400,
        FM4_ModelSize = 0x00000800,
        FM4_Level = 0x00004000, //Champion Level
        FM4_Unk = 0x00010000 //Unk -> Transparent-ish Life bar when changed:: MAYBE IF UNIT IS TARGETABLE
    }

    public class StatMod
    {
        public MasterMask BlockId { get; set; }
        public FieldMask Mask { get; set; }
        public float Value { get; set; }

        public static StatMod FromValues(MasterMask blockId, FieldMask mask, float value)
        {
            return new StatMod
            {
                BlockId = blockId,
                Mask = mask,
                Value = value
            };
        }
    }

    public class Stats
    {
        protected Dictionary<MasterMask, Dictionary<FieldMask, float>> _updatedStats = new Dictionary<MasterMask, Dictionary<FieldMask, float>>();

        protected float _gold;
        protected float _level;
        protected float _experience;
        protected float _currentHealth;
        protected float _currentMana;
        protected float _spellsEnabled;
        protected float _summonerSpellsEnabled;

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

        public virtual float Gold {
            get { return _gold; }
            set
            {
                _gold = value;
                appendStat(_updatedStats, MasterMask.MM_One, FieldMask.FM1_Gold, _gold);
            }
        }

        public virtual byte Level {
            get { return (byte)(Math.Floor(_level) + 0.5f); }
            set
            {
                _level = value;
                appendStat(_updatedStats, MasterMask.MM_Four, FieldMask.FM4_Level, _level);
            }
        }

        public virtual float Experience
        {
            get { return _experience; }
            set
            {
                _experience = value;
                appendStat(_updatedStats, MasterMask.MM_Four, FieldMask.FM4_exp, _experience);
            }
        }

        public virtual float CurrentHealth
        {
            get { return _currentHealth; }
            set
            {
                _currentHealth = value;
            }
        }

        public virtual float CurrentMana
        {
            get { return _currentMana; }
            set
            {
                _currentMana = value;
                appendStat(_updatedStats, MasterMask.MM_Four, FieldMask.FM4_CurrentMana, CurrentMana);
            }
        }

        protected bool generatingGold; // Used to determine if the Stats update should include generating gold. Changed in Champion.h
        protected float spellCostReduction; //URF Buff/Lissandra's passive
        protected float critDamagePct; //Default = 2... add with runes/items (change with yasuo's passive)

        public Stats()
        {
            spellCostReduction = 0;
            critDamagePct = 2;
            ManaCost = new float[4];

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
            if (AbilityPower.ApplyStatModificator(modifier.AbilityPower))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Base_Ap, AbilityPower.BaseValue);
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Bonus_Ap_Flat, AbilityPower.FlatBonus);
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Bonus_Ap_Pct, AbilityPower.PercentBonus);
            }
            if (Armor.ApplyStatModificator(modifier.Armor))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Armor, Armor.Total);
            }
            if (ArmorPenetration.ApplyStatModificator(modifier.ArmorPenetration))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Armor_Pen_Flat, ArmorPenetration.FlatBonus);
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Armor_Pen_Pct, ArmorPenetration.PercentBonus);
            }
            if (AttackDamage.ApplyStatModificator(modifier.AttackDamage))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Base_Ad, AttackDamage.BaseValue);
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Bonus_Ad_Flat, AttackDamage.FlatBonus);
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Bonus_Ad_Pct, AttackDamage.PercentBonus);
            }
            if (AttackSpeedMultiplier.ApplyStatModificator(modifier.AttackSpeed))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Atks_multiplier, AttackSpeedMultiplier.Total);
            }
            if (CriticalChance.ApplyStatModificator(modifier.CriticalChance))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Crit_Chance, CriticalChance.Total);
            }
            GoldPerSecond.ApplyStatModificator(modifier.GoldPerSecond);
            if (HealthPoints.ApplyStatModificator(modifier.HealthPoints))
            {
                appendStat(_updatedStats, MasterMask.MM_Four, FieldMask.FM4_MaxHp, HealthPoints.Total);
            }
            if (HealthRegeneration.ApplyStatModificator(modifier.HealthRegeneration))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Hp5, HealthRegeneration.Total);
            }
            if (LifeSteal.ApplyStatModificator(modifier.LifeSteel))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_LifeSteal, LifeSteal.Total);
            }
            if (MagicResist.ApplyStatModificator(modifier.MagicResist))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Magic_Armor, MagicResist.Total);
            }
            if (MagicPenetration.ApplyStatModificator(modifier.MagicPenetration))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Magic_Pen_Flat, MagicPenetration.FlatBonus);
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Magic_Pen_Pct, MagicPenetration.PercentBonus);
            }
            if (ManaPoints.ApplyStatModificator(modifier.ManaPoints))
            {
                appendStat(_updatedStats, MasterMask.MM_Four, FieldMask.FM4_MaxMp, ManaPoints.Total);
            }
            if (ManaRegeneration.ApplyStatModificator(modifier.ManaRegeneration))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Mp5, ManaRegeneration.Total);
            }
            if (MoveSpeed.ApplyStatModificator(modifier.MoveSpeed))
            {
                appendStat(_updatedStats, MasterMask.MM_Four, FieldMask.FM4_Speed, MoveSpeed.Total);
            }
            if (Range.ApplyStatModificator(modifier.Range))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Range, Range.Total);
            }
            if (Size.ApplyStatModificator(modifier.Size))
            {
                appendStat(_updatedStats, MasterMask.MM_Four, FieldMask.FM4_ModelSize, Size.Total);
            }
            if (SpellVamp.ApplyStatModificator(modifier.SpellVamp))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_SpellVamp, SpellVamp.Total);
            }
            if (Tenacity.ApplyStatModificator(modifier.Tenacity))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Tenacity, Tenacity.Total);
            }
        }

        public void RemoveModifier(IStatsModifier modifier)
        {
            if (AbilityPower.RemoveStatModificator(modifier.AbilityPower))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Base_Ap, AbilityPower.BaseValue);
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Bonus_Ap_Flat, AbilityPower.FlatBonus);
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Bonus_Ap_Pct, AbilityPower.PercentBonus);
            }
            if (Armor.RemoveStatModificator(modifier.Armor))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Armor, Armor.Total);
            }
            if (ArmorPenetration.RemoveStatModificator(modifier.ArmorPenetration))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Armor_Pen_Flat, ArmorPenetration.FlatBonus);
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Armor_Pen_Pct, ArmorPenetration.PercentBonus);
            }
            if (AttackDamage.RemoveStatModificator(modifier.AttackDamage))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Base_Ad, AttackDamage.BaseValue);
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Bonus_Ad_Flat, AttackDamage.FlatBonus);
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Bonus_Ad_Pct, AttackDamage.PercentBonus);
            }
            if (AttackSpeedMultiplier.RemoveStatModificator(modifier.AttackSpeed))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Atks_multiplier, AttackSpeedMultiplier.Total);
            }
            if (CriticalChance.RemoveStatModificator(modifier.CriticalChance))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Crit_Chance, CriticalChance.Total);
            }
            GoldPerSecond.RemoveStatModificator(modifier.GoldPerSecond);
            if (HealthPoints.RemoveStatModificator(modifier.HealthPoints))
            {
                appendStat(_updatedStats, MasterMask.MM_Four, FieldMask.FM4_MaxHp, HealthPoints.Total);
            }
            if (HealthRegeneration.RemoveStatModificator(modifier.HealthRegeneration))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Hp5, HealthRegeneration.Total);
            }
            if (LifeSteal.RemoveStatModificator(modifier.LifeSteel))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_LifeSteal, LifeSteal.Total);
            }
            if (MagicResist.RemoveStatModificator(modifier.MagicResist))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Magic_Armor, MagicResist.Total);
            }
            if (MagicPenetration.RemoveStatModificator(modifier.MagicPenetration))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Magic_Pen_Flat, MagicPenetration.FlatBonus);
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Magic_Pen_Pct, MagicPenetration.PercentBonus);
            }
            if (ManaPoints.RemoveStatModificator(modifier.ManaPoints))
            {
                appendStat(_updatedStats, MasterMask.MM_Four, FieldMask.FM4_MaxMp, ManaPoints.Total);
            }
            if (ManaRegeneration.RemoveStatModificator(modifier.ManaRegeneration))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Mp5, ManaRegeneration.Total);
            }
            if (MoveSpeed.RemoveStatModificator(modifier.MoveSpeed))
            {
                appendStat(_updatedStats, MasterMask.MM_Four, FieldMask.FM4_Speed, MoveSpeed.Total);
            }
            if (Range.RemoveStatModificator(modifier.Range))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Range, Range.Total);
            }
            if (Size.RemoveStatModificator(modifier.Size))
            {
                appendStat(_updatedStats, MasterMask.MM_Four, FieldMask.FM4_ModelSize, Size.Total);
            }
            if (SpellVamp.RemoveStatModificator(modifier.SpellVamp))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_SpellVamp, SpellVamp.Total);
            }
            if (Tenacity.RemoveStatModificator(modifier.Tenacity))
            {
                appendStat(_updatedStats, MasterMask.MM_Two, FieldMask.FM2_Tenacity, Tenacity.Total);
            }
        }

        public virtual float GetStat(MasterMask blockId, FieldMask stat)
        {
            if (blockId == MasterMask.MM_One)
            {
                switch (stat)
                {
                    case FieldMask.FM1_Gold:
                        return Gold;
                    case FieldMask.FM1_Spells_Enabled:
                        return _spellsEnabled;
                    case FieldMask.FM1_SummonerSpells_Enabled:
                        return _summonerSpellsEnabled;
                }
            }
            else if (blockId == MasterMask.MM_Two)
            {
                switch (stat)
                {
                    case FieldMask.FM2_Base_Ad:
                        return AttackDamage.BaseValue;
                    case FieldMask.FM2_Armor:
                        return Armor.Total;
                    case FieldMask.FM2_Armor_Pen_Flat:
                        return ArmorPenetration.FlatBonus;
                    case FieldMask.FM2_Atks_multiplier:
                        return AttackSpeedMultiplier.Total;
                    case FieldMask.FM2_Base_Ap:
                        return AbilityPower.BaseValue;
                    case FieldMask.FM2_Armor_Pen_Pct:
                        return ArmorPenetration.PercentBonus;
                    case FieldMask.FM2_Bonus_Ad_Flat:
                        return AttackDamage.FlatBonus;
                    case FieldMask.FM2_Bonus_Ad_Pct:
                        return AttackDamage.PercentBonus;
                    case FieldMask.FM2_Bonus_Ap_Flat:
                        return AbilityPower.FlatBonus;
                    case FieldMask.FM2_Bonus_Ap_Pct:
                        return AbilityPower.PercentBonus;
                    case FieldMask.FM2_Crit_Chance:
                        return CriticalChance.Total;
                    case FieldMask.FM2_Hp5:
                        return HealthRegeneration.Total;
                    case FieldMask.FM2_Mp5:
                        return ManaRegeneration.Total;
                    case FieldMask.FM2_LifeSteal:
                        return LifeSteal.Total;
                    case FieldMask.FM2_Magic_Pen_Flat:
                        return MagicPenetration.FlatBonus;
                    case FieldMask.FM2_Magic_Pen_Pct:
                        return MagicPenetration.PercentBonus;
                    case FieldMask.FM2_Magic_Armor:
                        return MagicResist.Total;
                    case FieldMask.FM2_Tenacity:
                        return Tenacity.Total;
                    case FieldMask.FM2_SpellVamp:
                        return SpellVamp.Total;

                }
            }
            else if (blockId == MasterMask.MM_Three)
            {
                switch (stat)
                {
                    case FieldMask.FM3_Armor_Pen_Pct:
                        return ArmorPenetration.PercentBonus;
                    case FieldMask.FM3_Magic_Pen_Pct:
                        return MagicPenetration.PercentBonus;
                }
            }
            else if (blockId == MasterMask.MM_Four)
            {
                switch (stat)
                {
                    case FieldMask.FM4_Level:
                        return Level + 0.5f;
                    case FieldMask.FM4_exp:
                        return Experience;
                    case FieldMask.FM4_CurrentHp:
                        return CurrentHealth;
                    case FieldMask.FM4_CurrentMana:
                        return CurrentMana;
                    case FieldMask.FM4_ModelSize:
                        return Size.Total;
                    case FieldMask.FM4_Speed:
                        return MoveSpeed.Total;
                }
            }

            return 0;
        }

        protected void appendStat(Dictionary<MasterMask, Dictionary<FieldMask, float>> collection, MasterMask blockId, FieldMask stat, float value)
        {
            if (!collection.ContainsKey(blockId))
                collection.Add(blockId, new Dictionary<FieldMask, float>());

            if (!collection[blockId].ContainsKey(stat))
                collection[blockId].Add(stat, value);
            else
                collection[blockId][stat] = value;
        }

        public bool IsGeneratingGold()
        {
            return generatingGold;
        }

        public void SetGeneratingGold(bool b)
        {
            generatingGold = b;
        }

        public Dictionary<MasterMask, Dictionary<FieldMask, float>> GetUpdatedStats()
        {
            var ret = new Dictionary<MasterMask, Dictionary<FieldMask, float>>();
            lock (_updatedStats)
            {
                foreach (var blocks in _updatedStats)
                {
                    if (!ret.ContainsKey(blocks.Key))
                        ret.Add(blocks.Key, new Dictionary<FieldMask, float>());
                    foreach (var stat in blocks.Value)
                        ret[blocks.Key].Add(stat.Key, stat.Value);
                }
            }
            return ret;
        }

        public virtual Dictionary<MasterMask, Dictionary<FieldMask, float>> GetAllStats()
        {
            var toReturn = new Dictionary<MasterMask, Dictionary<FieldMask, float>>();
            Dictionary<MasterMask, Dictionary<FieldMask, float>> stats = new Dictionary<MasterMask, Dictionary<FieldMask, float>>();
            appendStat(stats, MasterMask.MM_One, FieldMask.FM1_Gold, Gold);
            appendStat(stats, MasterMask.MM_One, FieldMask.FM1_Spells_Enabled, _spellsEnabled);
            appendStat(stats, MasterMask.MM_One, FieldMask.FM1_SummonerSpells_Enabled, _summonerSpellsEnabled);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_SpellVamp, SpellVamp.Total);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Tenacity, Tenacity.Total);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Base_Ap, AbilityPower.BaseValue);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Bonus_Ap_Flat, AbilityPower.FlatBonus);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Bonus_Ap_Pct, AbilityPower.PercentBonus);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Armor, Armor.Total);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Armor_Pen_Flat, ArmorPenetration.FlatBonus);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Armor_Pen_Pct, ArmorPenetration.PercentBonus);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Base_Ad, AttackDamage.BaseValue);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Bonus_Ad_Flat, AttackDamage.FlatBonus);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Bonus_Ad_Pct, AttackDamage.PercentBonus);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Atks_multiplier, AttackSpeedMultiplier.Total);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Crit_Chance, CriticalChance.Total);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_LifeSteal, LifeSteal.Total);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Magic_Armor, MagicResist.Total);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Magic_Pen_Flat, MagicPenetration.FlatBonus);
            appendStat(stats, MasterMask.MM_Two, FieldMask.FM2_Magic_Pen_Pct, MagicPenetration.PercentBonus);
            appendStat(stats, MasterMask.MM_Four, FieldMask.FM4_MaxMp, ManaPoints.Total);
            appendStat(stats, MasterMask.MM_Four, FieldMask.FM4_exp, Experience);
            appendStat(stats, MasterMask.MM_Four, FieldMask.FM4_Speed, MoveSpeed.Total);
            appendStat(stats, MasterMask.MM_Four, FieldMask.FM4_ModelSize, Size.Total);
            appendStat(stats, MasterMask.MM_Four, FieldMask.FM4_MaxHp, HealthPoints.Total);
            appendStat(stats, MasterMask.MM_Four, FieldMask.FM4_CurrentHp, CurrentHealth);
            appendStat(stats, MasterMask.MM_Four, FieldMask.FM4_CurrentMana, CurrentMana);

            foreach (var block in stats)
            {
                if (!toReturn.ContainsKey(block.Key))
                    toReturn.Add(block.Key, new Dictionary<FieldMask, float>());
                foreach (var stat in block.Value)
                    toReturn[block.Key].Add(stat.Key, stat.Value);
            }
            return toReturn;
        }

        public void ClearUpdatedStats()
        {
            _updatedStats.Clear();
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

        public virtual byte getSize(MasterMask blockId, FieldMask stat)
        {
            switch (blockId)
            {
                case MasterMask.MM_One:
                    switch (stat)
                    {
                        case FieldMask.FM1_Spells_Enabled:
                            return 2;
                        case FieldMask.FM1_SummonerSpells_Enabled:
                            return 2; // not 100% sure
                    }
                    break;
                case MasterMask.MM_Four:
                    switch (stat)
                    {
                        case FieldMask.FM4_Level:
                            return 1;
                    }
                    break;
            }
            return 4;
        }

        public virtual float getSpellCostReduction()
        {
            return spellCostReduction;
        }

        public virtual void setSpellCostReduction(float scr)
        {
            spellCostReduction = scr;
        }

        public virtual float getCritDamagePct()
        {
            return critDamagePct;
        }

        public virtual void setCritDamagePct(float critDmg)
        {
            critDamagePct = critDmg;
        }

        public virtual bool getSpellEnabled(byte id)
        {
            short mask = (short)Math.Floor(_spellsEnabled + 0.5f);
            return (mask & (1 << id)) != 0;
        }

        public virtual void setSpellEnabled(byte id, bool enabled)
        {
            short mask = (short)Math.Floor(_spellsEnabled + 0.5f);
            if (enabled)
                mask |= (short)(1 << id);
            else
                mask &= (short)(~(1 << id));
            _spellsEnabled = mask;
            appendStat(_updatedStats, MasterMask.MM_One, FieldMask.FM1_Spells_Enabled, _spellsEnabled);
        }

        public virtual bool getSummonerSpellEnabled(byte id)
        {
            short mask = (short)Math.Floor(_summonerSpellsEnabled + 0.5f);
            return (mask & (1 << id)) != 0;
        }

        public virtual void setSummonerSpellEnabled(byte id, bool enabled)
        {
            short mask = (short)Math.Floor(_summonerSpellsEnabled + 0.5f);
            if (enabled)
                mask |= (short)(16 << id);
            else
                mask &= (short)(~(16 << id));
            _summonerSpellsEnabled = (float) mask;
            appendStat(_updatedStats, MasterMask.MM_One, FieldMask.FM1_SummonerSpells_Enabled, _summonerSpellsEnabled);
        }
    }
}
