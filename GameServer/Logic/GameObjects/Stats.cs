using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Core.Logic;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public enum FieldMask : uint
    {
        FM1_Gold = 0x00000001,
        FM1_Gold_Total = 0x00000002,
        FM1_Spells_Enabled = 0x00000004, // Bits: 0-3 -> Q-R, 4-9 -> Items, 10 -> Trinket
        FM1_SummonerSpells_Enabled = 0x00000010, // Bits: 0 -> D, 1 -> F
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
            return new StatMod()
            {
                BlockId = blockId,
                Mask = mask,
                Value = value
            };
        }
    }

    public class Stats
    {
        protected PairList<int, float>[] stats = new PairList<int, float>[5];
        protected PairList<byte, List<int>> updatedStats = new PairList<byte, List<int>>();
        protected bool updatedHealth;

        // Here all the stats that don't have a bitmask
        protected float goldPerSecond;
        protected float healthPerLevel, manaPerLevel;
        protected float adPerLevel, armorPerLevel, magicArmorPerLevel;
        protected float hp5RegenPerLevel, mp5RegenPerLevel;
        protected float movementSpeedPercentageModifier;
        protected bool generatingGold; // Used to determine if the stats update should include generating gold. Changed in Champion.h

        protected float baseMovement, baseAttackSpeed;

        protected float spellCostReduction; //URF Buff/Lissandra's passive
        protected float critDamagePct; //Default = 2... add with runes/items (change with yasuo's passive)

        public Stats()
        {
            for (var i = 0; i < stats.Length; i++)
                stats[i] = new PairList<int, float>();

            updatedHealth = false;
            goldPerSecond = 0;
            healthPerLevel = 0;
            manaPerLevel = 0;
            adPerLevel = 0;
            armorPerLevel = 0;
            magicArmorPerLevel = 0;
            hp5RegenPerLevel = 0;
            mp5RegenPerLevel = 0;
            movementSpeedPercentageModifier = 0;
            baseMovement = 0;
            baseAttackSpeed = 0.625f;
            spellCostReduction = 0;
            critDamagePct = 2;
        }
        public float getStat(MasterMask blockId, MinionFieldMask stat)
        {
            return getStat((byte)blockId, (int)stat);
        }

        public float getStat(MasterMask blockId, FieldMask stat)
        {
            return getStat((byte)blockId, (int)stat);
        }

        public float getStat(byte blockId, int stat)
        {
            int block = -1;
            while (blockId > 0)
            {
                blockId = (byte)(blockId >> 1);
                ++block;
            }

            if (block >= 0 && block < stats.Length - 1)
            {
                var it = stats[block];
                if (it.ContainsKey(stat))
                    return it[stat];
            }
            return 0;
        }
        public void setStat(MasterMask blockId, MinionFieldMask stat, float value)
        {
            setStat((byte)blockId, (int)stat, value);
        }

        public void setStat(MasterMask blockId, FieldMask stat, float value)
        {
            setStat((byte)blockId, (int)stat, value);
        }

        public void setStat(byte blockId, int stat, float value)
        {
            int block = -1;
            if (!updatedStats.ContainsKey(blockId))
                updatedStats.Add(blockId, new List<int>());

            if (!updatedStats[blockId].Contains(stat))
                updatedStats[blockId].Add(stat);

            while (blockId > 0)
            {
                blockId = (byte)(blockId >> 1);
                ++block;
            }
            if (!(value > 0 || value < 0 || value == 0)) //NaN?
                System.Diagnostics.Debugger.Break();
            if (!stats[block].ContainsKey(stat))
                stats[block].Add(stat, value);
            else
                stats[block][stat] = value;

        }
        public bool isGeneratingGold()
        {
            return generatingGold;
        }

        public void setGeneratingGold(bool b)
        {
            generatingGold = b;
        }

        public PairList<byte, List<int>> getUpdatedStats()
        {
            var ret = new PairList<byte, List<int>>();
            lock (updatedStats)
            {
                foreach (var blocks in updatedStats)
                {
                    var retStats = new List<int>();
                    foreach (var stat in blocks.Item2)
                        retStats.Add(stat);
                    ret.Add(blocks.Item1, retStats);
                }
            }
            return ret;
        }

        public PairList<byte, List<int>> getAllStats()
        {
            var toReturn = new PairList<byte, List<int>>();

            for (byte i = 0; i < 5; ++i)
            {
                foreach (var kv in stats[i])
                {
                    if (!toReturn.ContainsKey((byte)(1 << i)))
                        toReturn.Add((byte)(1 << i), new List<int>());
                    toReturn[(byte)(1 << i)].Add(kv.Item1);
                }
            }
            return toReturn;
        }

        public void clearUpdatedStats()
        {
            updatedStats.Clear();
        }

        public bool isUpdatedHealth()
        {
            return updatedHealth;
        }

        public void clearUpdatedHealth()
        {
            updatedHealth = false;
        }

        public void update(long diff)
        {
            if (getHp5() > 0 && getCurrentHealth() < getMaxHealth())
            {
                float newHealth = getCurrentHealth() + (getHp5() * diff * 0.001f);
                newHealth = Math.Min(getMaxHealth(), newHealth);
                setCurrentHealth(newHealth);
            }

            if (getMana5() > 0 && getCurrentMana() < getMaxMana())
            {
                float newMana = getCurrentMana() + (getMana5() * diff * 0.001f);
                newMana = Math.Min(getMaxMana(), newMana);
                setCurrentMana(newMana);
            }
            if (generatingGold && getGoldPerSecond() > 0)
            {
                float newGold = getGold() + getGoldPerSecond() * (diff * 0.001f);
                setGold(newGold);
            }
        }
        public void levelUp()
        {
            setLevel(getLevel() + 1);

            setMaxHealth(getMaxHealth() + healthPerLevel);
            setCurrentHealth((getMaxHealth() / (getMaxHealth() - healthPerLevel)) * getCurrentHealth());
            setMaxMana(getMaxMana() + manaPerLevel);
            setCurrentMana((getMaxMana() / (getMaxMana() - manaPerLevel)) * getCurrentMana());
            setBaseAd(getBaseAd() + adPerLevel);
            setArmor(getArmor() + armorPerLevel);
            setMagicArmor(getMagicArmor() + magicArmorPerLevel);
            setHp5(getHp5() + hp5RegenPerLevel);
            setMp5(getMana5() + mp5RegenPerLevel);
        }

        public void applyStatMods(IEnumerable<StatMod> statMods)
        {
            foreach (var stat in statMods)
            {
                if (stat.Value == 0)
                    continue;

                setStat(stat.BlockId, stat.Mask, getStat(stat.BlockId, stat.Mask) + stat.Value);
            }
        }
        public void unapplyStatMods(List<StatMod> statMods)
        {
            foreach (var stat in statMods)
            {
                if (stat.Value == 0)
                    continue;

                setStat(stat.BlockId, stat.Mask, getStat(stat.BlockId, stat.Mask) - stat.Value);
            }
        }

        public virtual byte getSize(byte blockId, int stat)
        {
            switch (blockId)
            {
                case (byte)MasterMask.MM_One:
                    switch (stat)
                    {
                        case (int)FieldMask.FM1_Spells_Enabled:
                            return 2;
                        case (int)FieldMask.FM1_SummonerSpells_Enabled:
                            return 2; // not 100% sure
                    }
                    break;
                case (byte)MasterMask.MM_Four:
                    switch (stat)
                    {
                        case (int)FieldMask.FM4_Level:
                            return 1;
                    }
                    break;
            }
            return 4;
        }

        public virtual float getMovementSpeedPercentageModifier()
        {
            return 1.0f + (movementSpeedPercentageModifier / 100.0f);
        }

        public void setBaseMovementSpeed(float ms)
        {
            baseMovement = ms;
            setStat(MasterMask.MM_Four, FieldMask.FM4_Speed, baseMovement * getMovementSpeedPercentageModifier());
        }

        public void addMovementSpeedPercentageModifier(float amount)
        {
            movementSpeedPercentageModifier += amount;
            setStat(MasterMask.MM_Four, FieldMask.FM4_Speed, baseMovement * getMovementSpeedPercentageModifier());
        }

        public virtual float getBaseAd()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Base_Ad);
        }

        public virtual float getBonusAdFlat()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Bonus_Ad_Flat);
        }

        public virtual float getBonusApFlat()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Bonus_Ap_Flat);
        }

        public virtual float getBonusAdPct()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Bonus_Ad_Pct);
        }

        public virtual float getBaseAp()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Base_Ap);
        }

        public virtual float getCritChance()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Crit_Chance);
        }

        public virtual float getArmor()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Armor);
        }

        public virtual float getMagicArmor()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Magic_Armor);
        }

        public virtual float getRange()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Range);
        }

        public virtual float getArmorPenFlat()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Armor_Pen_Flat);
        }

        public virtual float getArmorPenPct()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Armor_Pen_Pct);
        }

        public virtual float getMagicPenFlat()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Magic_Pen_Flat);
        }

        public virtual float getMagicPenPct()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Magic_Pen_Pct);
        }

        public virtual float getCurrentHealth()
        {
            return getStat(MasterMask.MM_Four, FieldMask.FM4_CurrentHp);
        }

        public virtual float getCurrentMana()
        {
            return getStat(MasterMask.MM_Four, FieldMask.FM4_CurrentMana);
        }

        public virtual float getMaxHealth()
        {
            return getStat(MasterMask.MM_Four, FieldMask.FM4_MaxHp);
        }

        public virtual float getMaxMana()
        {
            return getStat(MasterMask.MM_Four, FieldMask.FM4_MaxMp);
        }

        public virtual float getHp5()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Hp5);
        }

        public virtual float getMana5()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Mp5);
        }

        public virtual float getCDR()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_cdr);
        }

        public virtual float getLifeSteal()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_LifeSteal);
        }

        public virtual float getSpellVamp()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_SpellVamp);
        }

        public virtual float getMovementSpeed()
        {
            //  CORE_INFO("Movement speed with buffs %f", getMovementSpeedPercentageModifier() * getStat(MasterMask.MM_Four, FieldMaskFour.FM4_Speed));
            return getStat(MasterMask.MM_Four, FieldMask.FM4_Speed);
        }

        public virtual float getBaseAttackSpeed()
        {
            return baseAttackSpeed;
        }

        public virtual float getAttackSpeedMultiplier()
        {
            return getStat(MasterMask.MM_Two, FieldMask.FM2_Atks_multiplier);
        }

        public virtual float getGold()
        {
            return getStat(MasterMask.MM_One, FieldMask.FM1_Gold);
        }

        public virtual float getGoldPerSecond()
        {
            return goldPerSecond;
        }

        public virtual byte getLevel()
        {
            return (byte)Math.Floor(getStat(MasterMask.MM_Four, FieldMask.FM4_Level) + 0.5f);
        }

        public virtual float getExperience()
        {
            return getStat(MasterMask.MM_Four, FieldMask.FM4_exp);
        }

        public virtual void setCritChance(float crit)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Crit_Chance, crit);
        }

        public virtual void setBaseAd(float ad)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Base_Ad, ad);
        }

        public virtual void setRange(float range)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Range, range);
        }

        public virtual void setBonusAdFlat(float ad)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Bonus_Ad_Flat, ad);
        }

        public virtual void setBonusApFlat(float ap)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Bonus_Ap_Flat, ap);
        }

        public virtual void setArmor(float armor)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Armor, armor);
        }

        public virtual void setMagicArmor(float armor)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Magic_Armor, armor);
        }

        public virtual void setHp5(float hp5)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Hp5, hp5);
        }

        public virtual void setMp5(float mp5)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Mp5, mp5);
        }

        public virtual void setArmorPenFlat(float pen)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Armor_Pen_Flat, pen);
        }

        public virtual void setArmorPenPct(float pen)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Armor_Pen_Pct, pen);
        }

        public virtual void setMagicPenFlat(float pen)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Magic_Pen_Flat, pen);
        }

        public virtual void setMagicPenPct(float pen)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Magic_Pen_Pct, pen);
        }

        public virtual void setCurrentHealth(float health)
        {
            setStat(MasterMask.MM_Four, FieldMask.FM4_CurrentHp, health);
            updatedHealth = true;
        }

        public virtual void setCurrentMana(float mana)
        {
            setStat(MasterMask.MM_Four, FieldMask.FM4_CurrentMana, mana);
        }

        public virtual void setMaxMana(float mana)
        {
            setStat(MasterMask.MM_Four, FieldMask.FM4_MaxMp, mana);
        }

        public virtual void setMaxHealth(float health)
        {
            setStat(MasterMask.MM_Four, FieldMask.FM4_MaxHp, health);
            updatedHealth = true;
        }

        public virtual void setMovementSpeed(float speed)
        {
            setStat(MasterMask.MM_Four, FieldMask.FM4_Speed, speed);
        }

        public virtual void setGold(float gold)
        {
            setStat(MasterMask.MM_One, FieldMask.FM1_Gold, gold);
        }

        public virtual void setGoldPerSecond(float gold)
        {
            goldPerSecond = gold;
        }

        public virtual void setBaseAp(float AP)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Base_Ap, AP);
        }

        public virtual void setExp(float EXP)
        {
            setStat(MasterMask.MM_Four, FieldMask.FM4_exp, EXP);
        }

        public virtual void setLevel(float level)
        {
            setStat(MasterMask.MM_Four, FieldMask.FM4_Level, level);
        }

        public virtual void setSize(float Size)
        {
            setStat(MasterMask.MM_Four, FieldMask.FM4_ModelSize, Size);
        }

        public virtual void setLifeSteal(float lifeSteal)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_LifeSteal, lifeSteal);
        }

        public virtual void setSpellVamp(float spellVamp)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_SpellVamp, spellVamp);
        }

        public virtual void setBaseAttackSpeed(float speed)
        {
            baseAttackSpeed = speed;
        }

        public virtual void setAttackSpeedMultiplier(float multiplier)
        {
            setStat(MasterMask.MM_Two, FieldMask.FM2_Atks_multiplier, multiplier);
        }

        public virtual void setAdPerLevel(float ad)
        {
            adPerLevel = ad;
        }

        public virtual void setHealthPerLevel(float health)
        {
            healthPerLevel = health;
        }

        public virtual void setManaPerLevel(float mana)
        {
            manaPerLevel = mana;
        }

        public virtual void setArmorPerLevel(float armor)
        {
            armorPerLevel = armor;
        }

        public virtual void setMagicArmorPerLevel(float magicArmor)
        {
            magicArmorPerLevel = magicArmor;
        }

        public virtual void setHp5RegenPerLevel(float hpRegen)
        {
            hp5RegenPerLevel = hpRegen;
        }

        public virtual void setMp5RegenPerLevel(float mpRegen)
        {
            mp5RegenPerLevel = mpRegen;
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
            short mask = (short)Math.Floor(getStat(MasterMask.MM_One, FieldMask.FM1_Spells_Enabled) + 0.5f);
            return (mask & (1 << id)) != 0;
        }

        public virtual void setSpellEnabled(byte id, bool enabled)
        {
            short mask = (short)Math.Floor(getStat(MasterMask.MM_One, FieldMask.FM1_Spells_Enabled) + 0.5f);
            if (enabled)
                mask |= (short)(1 << id);
            else
                mask |= (short)(~(1 << id));
            setStat(MasterMask.MM_One, FieldMask.FM1_Spells_Enabled, (float)mask);
        }

        public virtual bool getSummonerSpellEnabled(byte id)
        {
            short mask = (short)Math.Floor(getStat(MasterMask.MM_One, FieldMask.FM1_SummonerSpells_Enabled) + 0.5f);
            return (mask & (1 << id)) != 0;
        }

        public virtual void setSummonerSpellEnabled(byte id, bool enabled)
        {
            short mask = (short)Math.Floor(getStat(MasterMask.MM_One, FieldMask.FM1_SummonerSpells_Enabled) + 0.5f);
            if (enabled)
                mask |= (short)(16 << id);
            else
                mask &= (short)(~(16 << id));
            setStat(MasterMask.MM_One, FieldMask.FM1_SummonerSpells_Enabled, (float)mask);
        }

        /**
         * Meta-stats, relying on other stats
         */

        public float getTotalAd()
        {
            return (getBaseAd() + getBonusAdFlat()) * (1 + getBonusAdPct());
        }

        public float getTotalAttackSpeed()
        {
            return getBaseAttackSpeed() * getAttackSpeedMultiplier();
        }

        public float getTotalMovementSpeed()
        {
            return getMovementSpeedPercentageModifier() * baseMovement;
        }

        public float getTotalAp()
        {
            return (getBaseAp() + getBonusApFlat()); //Where's AP pctg? for stuff like Rabadon's
        }
    }
}
