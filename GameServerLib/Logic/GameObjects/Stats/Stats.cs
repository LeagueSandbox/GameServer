using LeagueSandbox.GameServer.Logic.Content;
using System;
using LeagueSandbox.GameServer.Logic.Enet;

namespace LeagueSandbox.GameServer.Logic.GameObjects
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
        public IsTargetableToTeamFlags IsTargetableToTeam { get; set; }

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
        public Stat CriticalDamage { get; }
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
        protected float _spellCostReduction; //URF Buff/Lissandra's passive
        public float SpellCostReduction
        {
            get => Spell.ManaCostsEnabled ? _spellCostReduction : 1;
            set => _spellCostReduction = value;
        }

        public Stats()
        {
            _spellCostReduction = 0;
            ManaCost = new float[64];
            ActionState = ActionState.CanAttack | ActionState.CanCast | ActionState.CanMove | ActionState.Unknown;
            IsTargetable = true;
            IsTargetableToTeam = IsTargetableToTeamFlags.TargetableToAll;

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
            ParType = charData.PARType;
        }

        public void AddModifier(StatsModifier modifier)
        {
            AbilityPower.ApplyStatModificator(modifier.AbilityPower);
            Armor.ApplyStatModificator(modifier.Armor);
            ArmorPenetration.ApplyStatModificator(modifier.ArmorPenetration);
            AttackDamage.ApplyStatModificator(modifier.AttackDamage);
            AttackSpeedMultiplier.ApplyStatModificator(modifier.AttackSpeed);
            CriticalChance.ApplyStatModificator(modifier.CriticalChance);
            CriticalDamage.ApplyStatModificator(modifier.CriticalDamage);
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

        public void RemoveModifier(StatsModifier modifier)
        {
            AbilityPower.RemoveStatModificator(modifier.AbilityPower);
            Armor.RemoveStatModificator(modifier.Armor);
            ArmorPenetration.RemoveStatModificator(modifier.ArmorPenetration);
            AttackDamage.RemoveStatModificator(modifier.AttackDamage);
            AttackSpeedMultiplier.RemoveStatModificator(modifier.AttackSpeed);
            CriticalChance.RemoveStatModificator(modifier.CriticalChance);
            CriticalDamage.RemoveStatModificator(modifier.CriticalDamage);
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

            HealthPoints.BaseValue += HealthPerLevel;
            CurrentHealth = HealthPoints.Total / (HealthPoints.Total - HealthPerLevel) * CurrentHealth;
            ManaPoints.BaseValue = ManaPoints.Total + ManaPerLevel;
            CurrentMana = ManaPoints.Total / (ManaPoints.Total - ManaPerLevel) * CurrentMana;
            AttackDamage.BaseValue = AttackDamage.BaseValue + AdPerLevel;
            Armor.BaseValue = Armor.BaseValue + ArmorPerLevel;
            MagicResist.BaseValue = MagicResist.Total + MagicResistPerLevel;
            HealthRegeneration.BaseValue = HealthRegeneration.BaseValue + HealthRegenerationPerLevel;
            ManaRegeneration.BaseValue = ManaRegeneration.BaseValue + ManaRegenerationPerLevel;
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

    [Flags]
    public enum ActionState : uint
    {
        CanAttack = 1 << 0,
        CanCast = 1 << 1,
        CanMove = 1 << 2,
        CanNotMove = 1 << 3,
        Stealthed = 1 << 4,
        RevealSpecificUnit = 1 << 5,
        Taunted = 1 << 6,
        Feared = 1 << 7,
        IsFleeing = 1 << 8,
        CanNotAttack = 1 << 9,
        IsAsleep = 1 << 10,
        IsNearSighted = 1 << 11,
        IsGhosted = 1 << 12,

        Charmed = 1 << 15,
        NoRender = 1 << 16,
        ForceRenderParticles = 1 << 17,
        
        Unknown = 1 << 23 // set to 1 by default, interferes with targetability
    }

    [Flags]
    public enum IsTargetableToTeamFlags : uint
    {
        NonTargetableAlly = 0x800000,
        NonTargetableEnemy = 0x1000000,
        TargetableToAll = 0x2000000
    }

    public enum PrimaryAbilityResourceType : byte
    {
        Mana = 0,
        Energy = 1,
        None = 2,
        Shield = 3,
        BattleFury = 4,
        DragonFury = 5,
        Rage = 6,
        Heat = 7,
        Ferocity = 8,
        BloodWell = 9,
        Wind = 10,
        Other = 11
    }
}
