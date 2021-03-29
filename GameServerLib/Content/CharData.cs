using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using GameServerCore.Domain;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Logging;
using log4net;
using Newtonsoft.Json;

namespace LeagueSandbox.GameServer.Content
{
    // TODO: Add Constants.var files to each Map's Content folder and assign values by reading them, currently this Data is only for Map1 as a placeholder.
    public class GlobalData : IGlobalData
    {
        public float AttackDelay { get; set; } = 1.600f;
        public float AttackDelayCastPercent { get; set; } = 0.300f;
        public float AttackMinDelay { get; set; } = 0.400f;
        public float PercentAttackSpeedModMinimum { get; set; } = -0.950f;
        public float AttackMaxDelay { get; set; } = 5.000f;
        public float CooldownMinimum { get; set; } = 0.000f;
        public float PercentRespawnTimeModMinimum { get; set; } = -0.950f;
        public float PercentGoldLostOnDeathModMinimum { get; set; } = -0.950f;
        public float PercentEXPBonusMinimum { get; set; } = -1.000f;
        public float PercentEXPBonusMaximum { get; set; } = 5.000f;
    }

    public class PassiveData : IPassiveData
    {
        public string PassiveAbilityName { get; set; } = "";
        public int[] PassiveLevels { get; set; } = new int[6];
        public string PassiveLuaName { get; set; } = "";
        public string PassiveNameStr { get; set; } = "";

        //TODO: Extend into handling several passives, when we decide on a format for that case.
        public static string GetPassiveAbilityNameFromScriptFile(string champName, List<IPackage> packages)
        {
            foreach (var package in packages)
            {
                var path = $"{package.PackagePath}\\Champions\\{champName}\\Passive.cs";
                if (File.Exists(path))
                {
                    var inputPassiveFile = File.ReadAllText(path);
                    string pattern = @"class (?<passiveName>\w+) : IGameScript";
                    RegexOptions options = RegexOptions.Multiline;
                    var passiveName = Regex.Match(inputPassiveFile, pattern, options).Groups["passiveName"].Value;

                    return passiveName;
                }
            }
            return "";
        }
    }

    public class CharData : ICharData
    {
        private readonly ContentManager _contentManager;
        private readonly ILog _logger;

        public CharData(ContentManager contentManager)
        {
            _contentManager = contentManager;
            _logger = LoggerProvider.GetLogger();
        }

        public IGlobalData GlobalCharData { get; private set; } = new GlobalData();

        public float BaseHp { get; private set; } = 100.0f;
        public float BaseMp { get; private set; } = 100.0f;
        public float BaseDamage { get; private set; } = 10.0f;
        public float AttackRange { get; private set; } = 100.0f;
        public int MoveSpeed { get; private set; } = 100;
        public float Armor { get; private set; } = 1.0f;
        public float SpellBlock { get; private set; }
        public float BaseStaticHpRegen { get; private set; } = 0.30000001f;
        public float BaseStaticMpRegen { get; private set; } = 0.30000001f;
        public float[] AttackDelayOffsetPercent { get; private set; } = new float[18];
        public float[] AttackDelayCastOffsetPercent { get; private set; } = new float[18];
        public float[] AttackDelayCastOffsetPercentAttackSpeedRatio { get; private set; } = new float[18];
        public float HpPerLevel { get; private set; } = 10.0f;
        public float MpPerLevel { get; private set; } = 10.0f;
        public float DamagePerLevel { get; private set; } = 10.0f;
        public float ArmorPerLevel { get; private set; } = 1.0f;
        public float SpellBlockPerLevel { get; private set; }
        public float HpRegenPerLevel { get; private set; }
        public float MpRegenPerLevel { get; private set; }
        public float AttackSpeedPerLevel { get; private set; }
        public bool IsMelee { get; private set; } //Yes or no
        public float PathfindingCollisionRadius { get; private set; } = -1.0f;
        public float PerceptionBubbleRadius { get; private set; } = 0.0f;
        public float GameplayCollisionRadius { get; private set; } = 65.0f;
        public PrimaryAbilityResourceType ParType { get; private set; } = PrimaryAbilityResourceType.MANA;

        public string[] SpellNames { get; private set; } = new string[4];
        public string[] ExtraSpells { get; private set; } = new string[16];
        public int[] MaxLevels { get; private set; } = { 5, 5, 5, 3 };

        public int[][] SpellsUpLevels { get; private set; } =
        {
            new[] {1, 3, 5, 7, 9, 99},
            new[] {1, 3, 5, 7, 9, 99},
            new[] {1, 3, 5, 7, 9, 99},
            new[] {6, 11, 16, 99, 99, 99}
        };

        public string[] AttackNames { get; private set; } = new string[18];
        public float[] AttackProbabilities { get; private set; } = new float[18];

        // TODO: Verify if we want this to be an array.
        public IPassiveData Passive { get; private set; } = new PassiveData();

        public void Load(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var file = new ContentFile();
            List<IPackage> packages;
            try
            {
                file = (ContentFile)_contentManager.GetContentFileFromJson("Stats", name);
                packages = new List<IPackage>(_contentManager.GetAllLoadedPackages());
            }
            catch (ContentNotFoundException exception)
            {
                _logger.Warn(exception.Message);
                return;
            }

            BaseHp = file.GetFloat("Data", "BaseHP", BaseHp);
            BaseMp = file.GetFloat("Data", "BaseMP", BaseMp);
            BaseDamage = file.GetFloat("Data", "BaseDamage", BaseDamage);
            AttackRange = file.GetFloat("Data", "AttackRange", AttackRange);
            MoveSpeed = file.GetInt("Data", "MoveSpeed", MoveSpeed);
            Armor = file.GetFloat("Data", "Armor", Armor);
            SpellBlock = file.GetFloat("Data", "SpellBlock", SpellBlock);
            BaseStaticHpRegen = file.GetFloat("Data", "BaseStaticHPRegen", BaseStaticHpRegen);
            BaseStaticMpRegen = file.GetFloat("Data", "BaseStaticMPRegen", BaseStaticMpRegen);
            HpPerLevel = file.GetFloat("Data", "HPPerLevel", HpPerLevel);
            MpPerLevel = file.GetFloat("Data", "MPPerLevel", MpPerLevel);
            DamagePerLevel = file.GetFloat("Data", "DamagePerLevel", DamagePerLevel);
            ArmorPerLevel = file.GetFloat("Data", "ArmorPerLevel", ArmorPerLevel);
            SpellBlockPerLevel = file.GetFloat("Data", "SpellBlockPerLevel", SpellBlockPerLevel);
            HpRegenPerLevel = file.GetFloat("Data", "HPRegenPerLevel", HpRegenPerLevel);
            MpRegenPerLevel = file.GetFloat("Data", "MPRegenPerLevel", MpRegenPerLevel);
            AttackSpeedPerLevel = file.GetFloat("Data", "AttackSpeedPerLevel", AttackSpeedPerLevel);
            IsMelee = file.GetString("Data", "IsMelee", IsMelee ? "true" : "false").Equals("true");
            PathfindingCollisionRadius = file.GetFloat("Data", "PathfindingCollisionRadius", PathfindingCollisionRadius);
            PerceptionBubbleRadius = file.GetFloat("Data", "PerceptionBubbleRadius", PerceptionBubbleRadius);
            GameplayCollisionRadius = file.GetFloat("Data", "GameplayCollisionRadius", GameplayCollisionRadius);
            Enum.TryParse<PrimaryAbilityResourceType>(file.GetString("Data", "PARType", ParType.ToString()),
                out var tempPar);
            ParType = tempPar;

            for (var i = 0; i < 4; i++)
            {
                SpellNames[i] = file.GetString("Data", $"Spell{i + 1}", "");
            }

            for (var i = 0; i < 16; i++)
            {
                ExtraSpells[i] = file.GetString("Data", $"ExtraSpell{i + 1}", "");
            }

            for (var i = 0; i < 4; i++)
            {
                SpellsUpLevels[i] = file.GetIntArray("Data", $"SpellsUpLevels{i + 1}", SpellsUpLevels[i]);
            }

            MaxLevels = file.GetIntArray("Data", "MaxLevels", MaxLevels);

            for (var i = 0; i < 18; i++)
            {
                if (i < 9)
                {
                    if (i == 0)
                    {
                        AttackNames[i] = name + "BasicAttack";
                        AttackProbabilities[i] = file.GetFloat("Data", "BaseAttack_Probability", 1.0f);
                        float initAttackCastTime = file.GetFloat("Data", "AttackCastTime", 0.0f);
                        float initAttackDelayOffsetPercent = file.GetFloat("Data", "AttackDelayOffsetPercent", 0.0f);
                        float initAttackDelayCastOffsetPercent = file.GetFloat("Data", "AttackDelayCastOffsetPercent", 0.0f);
                        float initAttackDelayCastOffsetPercentAttackSpeedRatio = file.GetFloat("Data", "AttackDelayCastOffsetPercentAttackSpeedRatio", 1.0f);
                        float initAttackTotalTime = file.GetFloat("Data", "AttackTotalTime", 0.0f);
                        float attackCastTime = Math.Min(initAttackTotalTime, initAttackCastTime);
                        if (initAttackTotalTime > 0.0f && attackCastTime > 0.0f)
                        {
                            AttackDelayOffsetPercent[i] = (initAttackTotalTime / GlobalCharData.AttackDelay) - 1.0f;
                            AttackDelayCastOffsetPercent[i] = (attackCastTime / initAttackTotalTime) - GlobalCharData.AttackDelayCastPercent;
                            AttackDelayCastOffsetPercentAttackSpeedRatio[i] = 1.0f;
                        }
                        else
                        {
                            AttackDelayOffsetPercent[i] = initAttackDelayOffsetPercent;
                            AttackDelayCastOffsetPercent[i] = Math.Max(initAttackDelayCastOffsetPercent, -GlobalCharData.AttackDelayCastPercent);
                            AttackDelayCastOffsetPercentAttackSpeedRatio[i] = initAttackDelayCastOffsetPercentAttackSpeedRatio;
                        }
                        continue;
                    }
                    else
                    {
                        AttackNames[i] = file.GetString("Data", $"ExtraAttack{i}", "");
                        AttackProbabilities[i] = file.GetFloat("Data", $"ExtraAttack{i}_Probability", 0.0f);
                        float extraAttackCastTime = file.GetFloat("Data", AttackNames[i] + "_AttackCastTime", 0.0f);
                        float extraAttackDelayOffsetPercent = file.GetFloat("Data", AttackNames[i] + "_AttackDelayOffsetPercent", AttackDelayOffsetPercent[0]);
                        float extraAttackDelayCastOffsetPercent = file.GetFloat("Data", AttackNames[i] + "_AttackDelayCastOffsetPercent", AttackDelayCastOffsetPercent[0]);
                        float extraAttackDelayCastOffsetPercentAttackSpeedRatio = file.GetFloat("Data", AttackNames[i] + "_AttackDelayCastOffsetPercentAttackSpeedRatio", 1.0f);
                        float extraAttackTotalTime = file.GetFloat("Data", AttackNames[i] + "_AttackTotalTime", 0.0f);
                        float attackCastTime = Math.Min(extraAttackTotalTime, extraAttackCastTime);
                        if (extraAttackTotalTime > 0.0f && attackCastTime > 0.0f)
                        {
                            AttackDelayOffsetPercent[i] = (extraAttackTotalTime / GlobalCharData.AttackDelay) - 1.0f;
                            AttackDelayCastOffsetPercent[i] = (attackCastTime / extraAttackTotalTime) - GlobalCharData.AttackDelayCastPercent;
                            AttackDelayCastOffsetPercentAttackSpeedRatio[i] = 1.0f;
                        }
                        else
                        {
                            AttackDelayOffsetPercent[i] = extraAttackDelayOffsetPercent;
                            AttackDelayCastOffsetPercent[i] = Math.Max(extraAttackDelayCastOffsetPercent, -GlobalCharData.AttackDelayCastPercent);
                            AttackDelayCastOffsetPercentAttackSpeedRatio[i] = extraAttackDelayCastOffsetPercentAttackSpeedRatio;
                        }
                    }
                }
                else if (i == 9)
                {
                    AttackNames[i] = name + "CritAttack";
                    AttackProbabilities[i] = file.GetFloat("Data", $"CritAttack_Probability", 1.0f);
                    float initAttackCastTime = file.GetFloat("Data", "CritAttack_AttackCastTime", 0.0f);
                    float initAttackDelayOffsetPercent = file.GetFloat("Data", "CritAttack_AttackDelayOffsetPercent", AttackDelayOffsetPercent[0]);
                    float initAttackDelayCastOffsetPercent = file.GetFloat("Data", "CritAttack_AttackDelayCastOffsetPercent", AttackDelayCastOffsetPercent[0]);
                    float initAttackDelayCastOffsetPercentAttackSpeedRatio = file.GetFloat("Data", "CritAttack_AttackDelayCastOffsetPercentAttackSpeedRatio", 1.0f);
                    float initAttackTotalTime = file.GetFloat("Data", "CritAttack_AttackTotalTime", 0.0f);
                    float attackCastTime = Math.Min(initAttackTotalTime, initAttackCastTime);
                    if (initAttackTotalTime > 0.0f && attackCastTime > 0.0f)
                    {
                        AttackDelayOffsetPercent[i] = (initAttackTotalTime / GlobalCharData.AttackDelay) - 1.0f;
                        AttackDelayCastOffsetPercent[i] = (attackCastTime / initAttackTotalTime) - GlobalCharData.AttackDelayCastPercent;
                        AttackDelayCastOffsetPercentAttackSpeedRatio[i] = 1.0f;
                    }
                    else
                    {
                        AttackDelayOffsetPercent[i] = initAttackDelayOffsetPercent;
                        AttackDelayCastOffsetPercent[i] = Math.Max(initAttackDelayCastOffsetPercent, -GlobalCharData.AttackDelayCastPercent);
                        AttackDelayCastOffsetPercentAttackSpeedRatio[i] = initAttackDelayCastOffsetPercentAttackSpeedRatio;
                    }
                    continue;
                }
                else if (AttackNames[9] != null)
                {
                    AttackNames[i] = file.GetString("Data", $"ExtraCritAttack{i}", "");
                    AttackProbabilities[i] = file.GetFloat("Data", $"ExtraCritAttack{i}_Probability", 0.0f);
                    float extraAttackCastTime = file.GetFloat("Data", AttackNames[i] + "_AttackCastTime", 0.0f);
                    float extraAttackDelayOffsetPercent = file.GetFloat("Data", AttackNames[i] + "_AttackDelayOffsetPercent", AttackDelayOffsetPercent[0]);
                    float extraAttackDelayCastOffsetPercent = file.GetFloat("Data", AttackNames[i] + "_AttackDelayCastOffsetPercent", AttackDelayCastOffsetPercent[0]);
                    float extraAttackDelayCastOffsetPercentAttackSpeedRatio = file.GetFloat("Data", AttackNames[i] + "_AttackDelayCastOffsetPercentAttackSpeedRatio", 1.0f);
                    float extraAttackTotalTime = file.GetFloat("Data", AttackNames[i] + "_AttackTotalTime", 0.0f);
                    float attackCastTime = Math.Min(extraAttackTotalTime, extraAttackCastTime);
                    if (extraAttackTotalTime > 0.0f && attackCastTime > 0.0f)
                    {
                        AttackDelayOffsetPercent[i] = (extraAttackTotalTime / GlobalCharData.AttackDelay) - 1.0f;
                        AttackDelayCastOffsetPercent[i] = (attackCastTime / extraAttackTotalTime) - GlobalCharData.AttackDelayCastPercent;
                        AttackDelayCastOffsetPercentAttackSpeedRatio[i] = 1.0f;
                    }
                    else
                    {
                        AttackDelayOffsetPercent[i] = extraAttackDelayOffsetPercent;
                        AttackDelayCastOffsetPercent[i] = Math.Max(extraAttackDelayCastOffsetPercent, -GlobalCharData.AttackDelayCastPercent);
                        AttackDelayCastOffsetPercentAttackSpeedRatio[i] = extraAttackDelayCastOffsetPercentAttackSpeedRatio;
                    }
                }
            }

            Passive.PassiveAbilityName = PassiveData.GetPassiveAbilityNameFromScriptFile(name, packages);
        }
    }
}
