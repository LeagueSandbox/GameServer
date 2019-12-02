﻿using System;
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
    public class PassiveData : IPassiveData
    {
        public string PassiveNameStr { get; set; } = "";
        public string PassiveAbilityName { get; set; } = "";
        public int[] PassiveLevels { get; set; } = { -1, -1, -1, -1, -1, -1 };

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

        public float BaseHp { get; private set; } = 100.0f;
        public float BaseMp { get; private set; } = 100.0f;
        public float BaseDamage { get; private set; } = 10.0f;
        public float AttackRange { get; private set; } = 100.0f;
        public int MoveSpeed { get; private set; } = 100;
        public float Armor { get; private set; } = 1.0f;
        public float SpellBlock { get; private set; }
        public float BaseStaticHpRegen { get; private set; } = 0.30000001f;
        public float BaseStaticMpRegen { get; private set; } = 0.30000001f;
        public float AttackDelayOffsetPercent { get; private set; }
        public float AttackDelayCastOffsetPercent { get; private set; }
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
        public float GameplayCollisionRadius { get; private set; } = 65.0f;
        public PrimaryAbilityResourceType ParType { get; private set; } = PrimaryAbilityResourceType.MANA;

        public string[] SpellNames { get; private set; } = { "", "", "", "" };
        public int[] MaxLevels { get; private set; } = { 5, 5, 5, 3 };

        public int[][] SpellsUpLevels { get; private set; } =
        {
            new[] {1, 3, 5, 7, 9, 99},
            new[] {1, 3, 5, 7, 9, 99},
            new[] {1, 3, 5, 7, 9, 99},
            new[] {6, 11, 16, 99, 99, 99}
        };

        public string[] ExtraSpells { get; private set; } = { "", "", "", "", "", "", "", "" };

        public IPassiveData[] Passives { get; private set; } =
        {
            new PassiveData(),
            new PassiveData(),
            new PassiveData(),
            new PassiveData(),
            new PassiveData(),
            new PassiveData()
        };

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
            AttackDelayOffsetPercent = file.GetFloat("Data", "AttackDelayOffsetPercent", AttackDelayOffsetPercent);
            AttackDelayCastOffsetPercent = file.GetFloat("Data", "AttackDelayCastOffsetPercent", AttackDelayCastOffsetPercent);
            HpPerLevel = file.GetFloat("Data", "HPPerLevel", HpPerLevel);
            MpPerLevel = file.GetFloat("Data", "MPPerLevel", MpPerLevel);
            DamagePerLevel = file.GetFloat("Data", "DamagePerLevel", DamagePerLevel);
            ArmorPerLevel = file.GetFloat("Data", "ArmorPerLevel", ArmorPerLevel);
            SpellBlockPerLevel = file.GetFloat("Data", "SpellBlockPerLevel", SpellBlockPerLevel);
            HpRegenPerLevel = file.GetFloat("Data", "HPRegenPerLevel", HpRegenPerLevel);
            MpRegenPerLevel = file.GetFloat("Data", "MPRegenPerLevel", MpRegenPerLevel);
            AttackSpeedPerLevel = file.GetFloat("Data", "AttackSpeedPerLevel", AttackSpeedPerLevel);
            IsMelee = file.GetString("Data", "IsMelee", IsMelee ? "true" : "false").Equals("true");
            PathfindingCollisionRadius =
                file.GetFloat("Data", "PathfindingCollisionRadius", PathfindingCollisionRadius);
            GameplayCollisionRadius = file.GetFloat("Data", "GameplayCollisionRadius", GameplayCollisionRadius);
            Enum.TryParse<PrimaryAbilityResourceType>(file.GetString("Data", "PARType", ParType.ToString()),
                out var tempPar);
            ParType = tempPar;

            for (var i = 0; i < 4; i++)
            {
                SpellNames[i] = file.GetString("Data", $"Spell{i + 1}", SpellNames[i]);
            }

            for (var i = 0; i < 4; i++)
            {
                SpellsUpLevels[i] = file.GetIntArray("Data", $"SpellsUpLevels{i + 1}", SpellsUpLevels[i]);
            }

            MaxLevels = file.GetIntArray("Data", "MaxLevels", MaxLevels);
            for (var i = 0; i < 8; i++)
            {
                ExtraSpells[i] = file.GetString("Data", $"ExtraSpell{i + 1}", ExtraSpells[i]);
            }

            for (var i = 0; i < 6; i++)
            {
                Passives[i].PassiveNameStr = file.GetString("Data", $"Passive{i + 1}Name", Passives[i].PassiveNameStr);
                Passives[i].PassiveAbilityName = PassiveData.GetPassiveAbilityNameFromScriptFile(name, packages);
                Passives[i].PassiveLevels = file.GetMultiInt("Data", $"Passive{i + 1}Level", 6, -1);
            }
        }
    }
}
