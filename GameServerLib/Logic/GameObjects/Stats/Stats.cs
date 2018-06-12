using System;
using System.Collections.Generic;
using System.IO;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Content;
using Newtonsoft.Json;

namespace LeagueSandbox.GameServer.Logic.GameObjects.Stats
{
    public class Stats
    {
        // Derived from AttackableUnit
        private float _currentHealth;
        public float Level1Health { get; set; }
        public float HealthGrowth { get; set; }
        private float _flatHealthBonus;
        private float _percentHealthBonus;
        public float CurrentHealth
        {
            get => _currentHealth.Clamp(0, TotalHealth);
            set => _currentHealth = value;
        }
        public float FlatHealthBonus
        {
            get => _flatHealthBonus;
            set
            {
                if (value == _flatHealthBonus)
                {
                    return;
                }

                var percentHp = CurrentHealthPercent;
                _flatHealthBonus = value;
                CurrentHealth = TotalHealth * percentHp;
            }
        }

        public float PercentHealthBonus
        {
            get => _percentHealthBonus;
            set
            {
                if (value == _percentHealthBonus)
                {
                    return;
                }

                var percentHp = CurrentHealthPercent;
                _percentHealthBonus = value;
                CurrentHealth = TotalHealth * percentHp;
            }
        }

        public float BaseHealth => Level1Health + (Level - 1) * HealthGrowth;
        public float TotalHealth => (BaseHealth + FlatHealthBonus) * (1 + PercentHealthBonus);
        public float CurrentHealthPercent => CurrentHealth / TotalHealth;

        public PrimaryAbilityResourceType PrimaryAbilityResourceType { get; set; } = PrimaryAbilityResourceType.Mana;
        private float _currentPar;
        public float Level1Par { get; set; }
        public float ParGrowth { get; set; }
        private float _flatParBonus;
        private float _percentParBonus;
        public float CurrentPar
        {
            get => _currentPar.Clamp(0, TotalPar);
            set => _currentPar = value;
        }

        public float FlatParBonus
        {
            get => _flatParBonus;
            set
            {
                if (value == _flatParBonus)
                {
                    return;
                }

                var percentHp = CurrentParPercent;
                _flatParBonus = value;
                CurrentPar = TotalPar * percentHp;
            }
        }
        public float PercentParBonus
        {
            get => _percentParBonus;
            set
            {
                if (value == _percentParBonus)
                {
                    return;
                }

                var percentHp = CurrentParPercent;
                _percentParBonus = value;
                CurrentPar = TotalPar * percentHp;
            }
        }

        public float BasePar => Level1Par + (Level - 1) * ParGrowth;
        public float TotalPar => (BasePar + FlatParBonus) * (1 + PercentParBonus);
        public float CurrentParPercent => CurrentPar / TotalPar;

        public bool IsInvulnerable { get; set; }
        public bool IsPhysicalImmune { get; set; }
        public bool IsMagicImmune { get; set; }
        public bool IsTargetable { get; set; } = true;
        public bool IsLifestealImmune { get; set; }
        public IsTargetableToTeamFlags IsTargetableToTeam { get; set; } = (IsTargetableToTeamFlags)0x2000000;
        
        // Derived from ObjAIBase
        public ActionState ActionState { get; set; } = (ActionState)0x800007;
        public float LifeTime { get; set; }
        public float MaxLifeTime { get; set; }
        public float LifeTimeTicks { get; set; }
        public bool IsMelee { get; set; }
        public Multiplicative PhysicalDamageReductionPercent { get; set; } = new Multiplicative();
        public Multiplicative MagicalDamageReductionPercent { get; set; } = new Multiplicative();

        // Derived from Champion
        public float Gold { get; set; }
        public float TotalGold { get; set; }
        public uint SpellEnabledBitFieldLower1 { get; set; } = uint.MaxValue;
        public uint SpellEnabledBitFieldUpper1 { get; set; } = uint.MaxValue;
        public uint SpellEnabledBitFieldLower2 { get; set; } = uint.MaxValue;
        public uint SpellEnabledBitFieldUpper2 { get; set; } = uint.MaxValue;
        public uint EvolvePoints { get; set; }
        public uint EvolveFlags { get; set; }
        public uint Level { get; set; }
        public uint NumberOfNeutralMinionsKilled { get; set; }
        public float PassiveCooldownEndTime { get; set; }
        public float PassiveCooldownTotalTime { get; set; }
        public float Experience { get; set; }
        public float PercentSpellCostReduction { get; set; }

        // General stat data

        // Movement Speed
        public float BaseMovementSpeed { get; set; }
        public float FlatMovementSpeedBonus { get; set; }
        public float AdditiveMovementSpeedBonus { get; set; }
        
        /// <summary>
        /// Only Galio E, Hecarim E, Master Yi R, Rammus Q, Kennen E, Quicksilver, Revive, Distortion + Flash and Heal are multiplicative.
        /// Everything else is additive.
        /// </summary>
        public Multiplicative MultiplicativeMovementSpeedBonus { get; set; } = new Multiplicative();
        public List<float> SlowsApplied { get; set; } = new List<float>();
        public float SlowResistPercent { get; set; }
        public float RawMovementSpeed
        {
            get
            {
                var total = (BaseMovementSpeed + FlatMovementSpeedBonus) * (1 + AdditiveMovementSpeedBonus) *
                            MultiplicativeMovementSpeedBonus;

                var slowRatio = 0f;
                if (SlowsApplied.Count > 0)
                {
                    SlowsApplied.Sort();
                    SlowsApplied.Reverse();
                    slowRatio = SlowsApplied[0];
                    for (var i = 1; i < SlowsApplied.Count; i++)
                    {
                        slowRatio *= 1 - SlowsApplied[i] * 0.35f;
                    }
                }

                total *= 1 - slowRatio * (1 - SlowResistPercent);

                return total;
            }
        }

        public float TotalMovementSpeed
        {
            get
            {
                var raw = RawMovementSpeed;
                
                // soft caps
                if (raw > 490)
                {
                    raw = raw * 0.5f + 230;
                }
                else if (raw > 415)
                {
                    raw = raw * 0.8f + 83;
                }
                else if (raw < 220)
                {
                    raw = raw * 0.5f + 110;
                }

                return raw;
            }
        }

        // Attack Speed
        public float AttackDelay { get; set; }
        public float BaseAttackSpeed => 0.625f / (1 + AttackDelay);
        public float PercentAttackSpeedMod { get; set; }
        public float AttackSpeedGrowth { get; set; }

        // seems like this gets affected by tenacity
        public Multiplicative PercentAttackSpeedDebuff { get; set; } = new Multiplicative();
        public float TotalAttackSpeed
        {
            get
            {
                var total = BaseAttackSpeed * (1 + (Level - 1) * AttackSpeedGrowth + PercentAttackSpeedMod) *
                            PercentAttackSpeedDebuff;

                return total.Clamp(0.2f, 2.5f);
            }
        }

        // Attack Range
        public float BaseAttackRange { get; set; }
        public float FlatAttackRangeMod { get; set; }
        public float PercentAttackRangeMod { get; set; }
        public float FlatAttackRangeDebuff { get; set; }
        public float PercentAttackRangeDebuff { get; set; }
        public float TotalAttackRange
        {
            get
            {
                var total = BaseAttackRange + FlatAttackRangeMod;
                total = total * 1 + PercentAttackRangeMod - FlatAttackRangeDebuff;
                return Math.Max(0, total * 1 - PercentAttackRangeDebuff);
            }
        }

        // Critical Chance
        public float FlatCriticalChanceMod { get; set; }
        public float CriticalChance => ((float)FlatCriticalChanceMod).Clamp(0, 1);

        // Critical Damage
        public float BaseCriticalDamage { get; set; } = 2;
        public float FlatCriticalDamageMod { get; set; }
        public Multiplicative PercentCriticalDamageMod { get; set; } = new Multiplicative();
        public float TotalCriticalDamage => (BaseCriticalDamage + FlatCriticalDamageMod) * PercentCriticalDamageMod;

        // Attack Damage
        public float Level1AttackDamage { get; set; }
        public float AttackDamageGrowth { get; set; }
        public float FlatAttackDamageMod { get; set; }
        public float PercentAttackDamageMod { get; set; }

        public float BaseAttackDamage => Level1AttackDamage + AttackDamageGrowth * (Level - 1);
        public float TotalAttackDamage => (BaseAttackDamage + FlatAttackDamageMod) * (1 + PercentAttackDamageMod);
        public float BonusAttackDamage => TotalAttackDamage - BaseAttackDamage;

        // Armor
        public float Level1Armor { get; set; }
        public float ArmorGrowth { get; set; }
        public float FlatArmorMod { get; set; }
        public float PercentArmorMod { get; set; }

        public float BaseArmor => Level1Armor + ArmorGrowth * (Level - 1);
        public float BonusArmor => TotalArmor - BaseArmor;
        public float TotalArmor
        {
            get
            {
                var total = (BaseArmor + FlatArmorMod) * (1 + PercentArmorMod) - FlatArmorReduction;
                if (total > 0)
                {
                    total *= PercentArmorReduction;
                }

                return total;
            }
        }

        // Armor Penetration
        public float FlatArmorReduction { get; set; }
        public Multiplicative PercentArmorReduction { get; set; } = new Multiplicative();
        public Multiplicative PercentArmorPenetration { get; set; } = new Multiplicative();
        public Multiplicative PercentBonusArmorPenetration { get; set; } = new Multiplicative();
        public float FlatArmorPenetration { get; set; }

        // Magic Resist
        public float Level1MagicResist { get; set; }
        public float MagicResistGrowth { get; set; }
        public float FlatMagicResistMod { get; set; }
        public float PercentMagicResistMod { get; set; }

        public float BaseMagicResist => Level1MagicResist + (Level - 1) * MagicResistGrowth;
        public float BonusMagicResist => TotalMagicResist - BaseMagicResist;
        public float TotalMagicResist
        {
            get
            {
                var total = (BaseMagicResist + FlatMagicResistMod) * (1 + PercentMagicResistMod) - FlatMagicReduction;
                if (total > 0)
                {
                    total *= PercentMagicReduction;
                }

                return total;
            }
        }

        // Magic Penetration
        public float FlatMagicReduction { get; set; }
        public Multiplicative PercentMagicReduction { get; set; } = new Multiplicative();
        public Multiplicative PercentMagicPenetration { get; set; } = new Multiplicative();
        public Multiplicative PercentBonusMagicPenetration { get; set; } = new Multiplicative();
        public float FlatMagicPenetration { get; set; }

        // Life Steal
        public float LifeSteal { get; set; }

        // Spell Vamp
        public float SpellVamp { get; set; }
        
        // Ability Power
        public float FlatAbilityPower { get; set; }
        public float PercentAbilityPower { get; set; }
        public float TotalAbilityPower => FlatAbilityPower * (1 + PercentAbilityPower);

        // Dodge Chance
        private float _dodgeChance;
        public float DodgeChance
        {
            get => _dodgeChance.Clamp(0, 1);
            set => _dodgeChance = value;
        }

        // Health Regeneration
        public float Level1HealthRegen { get; set; }
        public float HealthRegenGrowth { get; set; }
        public float FlatHealthRegenMod { get; set; }
        public float PercentBaseHealthRegenMod { get; set; }
        public float BaseHealthRegen => Level1HealthRegen + (Level - 1) * HealthRegenGrowth;
        public float TotalHealthRegen => BaseHealthRegen + FlatHealthRegenMod + PercentBaseHealthRegenMod * BaseHealthRegen;

        // Mana Regeneration
        public float Level1ParRegen { get; set; }
        public float ParRegenGrowth { get; set; }
        public float FlatParRegenMod { get; set; }
        public float PercentBaseParRegenMod { get; set; }
        public float BaseParRegen => Level1ParRegen + (Level - 1) * ParRegenGrowth;
        public float TotalParRegen => BaseParRegen + FlatParRegenMod + PercentBaseParRegenMod * BaseParRegen;

        // Cooldown Reduction
        public float CooldownReductionCap { get; set; } = 0.4f;
        public float FlatCooldownReduction { get; set; }
        public float CooldownReduction => Math.Min(CooldownReductionCap, FlatCooldownReduction);

        // Tenacity
        public Multiplicative Tenacity { get; set; } = new Multiplicative();

        // Pathfinding Radius
        public float FlatPathfindingRadiusMod { get; set; }

        // Size
        public float BaseSize { get; set; } = 1;
        public float FlatSizeMod { get; set; }
        public float PercentSizeMod { get; set; }
        public float TotalSize => (BaseSize + FlatSizeMod) * (1 + PercentSizeMod);

        public bool GetActionState(ActionState state)
        {
            return ActionState.HasFlag(state);
        }

        public void SetActionState(ActionState state, bool value)
        {
            if (value)
            {
                ActionState |= state;
            }
            else
            {
                ActionState &= ~state;
            }
        }

        public bool GetSpellEnabled(byte slot)
        {
            return (SpellEnabledBitFieldLower1 & (1 << slot)) != 0;
        }

        public void SetSpellEnabled(byte slot, bool enabled)
        {
            if (enabled)
            {
                SpellEnabledBitFieldLower1 |= (uint)(1 << slot);
            }
            else
            {
                SpellEnabledBitFieldLower1 &= (uint)~(1 << slot);
            }
        }

        public bool GetSummonerSpellEnabled(byte slot)
        {
            return (SpellEnabledBitFieldLower2 & (1 << slot)) != 0;
        }

        public void SetSummonerSpellEnabled(byte slot, bool enabled)
        {
            if (enabled)
            {
                SpellEnabledBitFieldLower2 |= (uint)(16 << slot);
            }
            else
            {
                SpellEnabledBitFieldLower2 &= (uint)~(16 << slot);
            }
        }

        public void LoadStats(string model, CharData charData, int skinId = 0)
        {
            Level1Health = charData.BaseHP;
            Level1Par = charData.BaseMP;
            Level1AttackDamage = charData.BaseDamage;
            BaseAttackRange = charData.AttackRange;
            BaseMovementSpeed = charData.MoveSpeed;
            Level1Armor = charData.Armor;
            Level1MagicResist = charData.SpellBlock;
            Level1HealthRegen = charData.BaseStaticHPRegen;
            Level1ParRegen = charData.BaseStaticMPRegen;
            AttackDelay = charData.AttackDelayOffsetPercent;
            HealthGrowth = charData.HPPerLevel;
            ParGrowth = charData.MPPerLevel;
            AttackDamageGrowth = charData.DamagePerLevel;
            ArmorGrowth = charData.ArmorPerLevel;
            MagicResistGrowth = charData.SpellBlockPerLevel;
            HealthRegenGrowth = charData.HPRegenPerLevel;
            ParRegenGrowth = charData.MPRegenPerLevel;
            AttackSpeedGrowth = charData.AttackSpeedPerLevel;

            CurrentHealth = TotalHealth;
            CurrentPar = TotalPar;

            var game = Program.ResolveDependency<Game>();
            var logger = Program.ResolveDependency<Logger>();
            var file = new ContentFile();
            try
            {
                var path = game.Config.ContentManager.GetUnitStatPath(model);
                logger.LogCoreInfo($"Loading {model}'s stats from path: {Path.GetFullPath(path)}!");
                var text = File.ReadAllText(Path.GetFullPath(path));
                file = JsonConvert.DeserializeObject<ContentFile>(text);
                if (file.Values.ContainsKey($"MeshSkin{(skinId == 0 ? "" : skinId.ToString())}"))
                {
                    BaseSize = file.GetFloat("MeshSkin", "SkinScale", 1);
                    return;
                }

                path = game.Config.ContentManager.GetUnitSkinPath(model, skinId);
                text = File.ReadAllText(Path.GetFullPath(path));
                file = JsonConvert.DeserializeObject<ContentFile>(text);
            }
            catch (ContentNotFoundException notfound)
            {
                logger.LogCoreWarning($"Stats for {model} was not found: {notfound.Message}");
                return;
            }

            BaseSize = file.GetFloat("MeshSkin", "SkinScale", 1);
        }

        public void ApplyItemValues(ItemType item)
        {
            FlatArmorMod += item.FlatArmorMod;
            PercentArmorMod += item.PercentArmorMod;
            FlatCriticalChanceMod += item.FlatCritChanceMod;
            FlatCriticalDamageMod += item.FlatCritDamageMod;
            PercentCriticalDamageMod.Add(item.PercentCritDamageMod);
            FlatHealthBonus += item.FlatHPPoolMod;
            PercentHealthBonus += item.PercentHPPoolMod;
            FlatAbilityPower += item.FlatMagicDamageMod;
            PercentAbilityPower += item.PercentMagicDamageMod;
            FlatMagicPenetration += item.FlatMagicPenetrationMod;
            FlatMovementSpeedBonus += item.FlatMovementSpeedMod;
            AdditiveMovementSpeedBonus += item.PercentMovementSpeedMod;
            FlatAttackDamageMod += item.FlatPhysicalDamageMod;
            PercentAttackDamageMod += item.PercentPhysicalDamageMod;
            FlatMagicResistMod += item.FlatSpellBlockMod;
            PercentMagicResistMod += item.PercentSpellBlockMod;
            PercentAttackSpeedMod += item.PercentAttackSpeedMod;
            PercentBaseHealthRegenMod += item.PercentBaseHPRegenMod;

            if (PrimaryAbilityResourceType == PrimaryAbilityResourceType.Mana)
            {
                FlatParBonus += item.FlatMPPoolMod;
                PercentParBonus += item.PercentMPPoolMod;
                PercentBaseParRegenMod += item.PercentBaseMPRegenMod;
            }
            else if (PrimaryAbilityResourceType == PrimaryAbilityResourceType.Energy)
            {
                PercentBaseParRegenMod += item.PercentBaseMPRegenMod;
            }
        }

        public void RemoveItemValues(ItemType item)
        {
            FlatArmorMod -= item.FlatArmorMod;
            PercentArmorMod -= item.PercentArmorMod;
            FlatCriticalChanceMod -= item.FlatCritChanceMod;
            FlatCriticalDamageMod -= item.FlatCritDamageMod;
            PercentCriticalDamageMod.Add(item.PercentCritDamageMod);
            FlatHealthBonus -= item.FlatHPPoolMod;
            PercentHealthBonus -= item.PercentHPPoolMod;
            FlatAbilityPower -= item.FlatMagicDamageMod;
            PercentAbilityPower -= item.PercentMagicDamageMod;
            FlatMagicPenetration -= item.FlatMagicPenetrationMod;
            FlatMovementSpeedBonus -= item.FlatMovementSpeedMod;
            AdditiveMovementSpeedBonus -= item.PercentMovementSpeedMod;
            FlatAttackDamageMod -= item.FlatPhysicalDamageMod;
            PercentAttackDamageMod -= item.PercentPhysicalDamageMod;
            FlatMagicResistMod -= item.FlatSpellBlockMod;
            PercentMagicResistMod -= item.PercentSpellBlockMod;
            PercentAttackSpeedMod -= item.PercentAttackSpeedMod;
            PercentBaseHealthRegenMod -= item.PercentBaseHPRegenMod;

            if (PrimaryAbilityResourceType == PrimaryAbilityResourceType.Mana)
            {
                FlatParBonus -= item.FlatMPPoolMod;
                PercentParBonus -= item.PercentMPPoolMod;
                PercentBaseParRegenMod -= item.PercentBaseMPRegenMod;
            }
            else if (PrimaryAbilityResourceType == PrimaryAbilityResourceType.Energy)
            {
                PercentBaseParRegenMod -= item.PercentBaseMPRegenMod;
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

        Unknown = 1 << 23 // set to 1 by default
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
