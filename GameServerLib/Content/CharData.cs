using System;
using System.IO;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Logging;
using log4net;
using Newtonsoft.Json;

namespace LeagueSandbox.GameServer.Content
{
    public class PassiveData
    {
        public string PassiveNameStr { get; set; } = "";
        public string PassiveLuaName { get; set; } = "";
        public int[] PassiveLevels { get; set; } = { -1, -1, -1, -1, -1, -1 };
    }

    public class CharData
    {
        private readonly Game _game;
        private readonly ILog _logger;

        public CharData(Game game)
        {
            _game = game;
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

        public PassiveData[] Passives { get; private set; } =
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
            try
            {
                var path = _game.Config.ContentManager.GetUnitStatPath(name);
                _logger.Debug($"Loading {name}'s Stats  from path: {Path.GetFullPath(path)}!");
                var text = File.ReadAllText(Path.GetFullPath(path));
                file = JsonConvert.DeserializeObject<ContentFile>(text);
            }
            catch (ContentNotFoundException notfound)
            {
                _logger.Warn($"Stats for {name} was not found: {notfound.Message}");
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
            HpPerLevel = file.GetFloat("Data", "HPPerLevel", HpPerLevel);
            MpPerLevel = file.GetFloat("Data", "MPPerLevel", MpPerLevel);
            DamagePerLevel = file.GetFloat("Data", "DamagePerLevel", DamagePerLevel);
            ArmorPerLevel = file.GetFloat("Data", "ArmorPerLevel", ArmorPerLevel);
            SpellBlockPerLevel = file.GetFloat("Data", "SpellBlockPerLevel", SpellBlockPerLevel);
            HpRegenPerLevel = file.GetFloat("Data", "HPRegenPerLevel", HpRegenPerLevel);
            MpRegenPerLevel = file.GetFloat("Data", "MPRegenPerLevel", MpRegenPerLevel);
            AttackSpeedPerLevel = file.GetFloat("Data", "AttackSpeedPerLevel", AttackSpeedPerLevel);
            IsMelee = file.GetString("Data", "IsMelee", IsMelee ? "Yes" : "No").Equals("yes");
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
                Passives[i].PassiveLuaName =
                    file.GetString("Data", $"Passive{i + 1}LuaName", Passives[i].PassiveLuaName);
                Passives[i].PassiveLevels = file.GetMultiInt("Data", $"Passive{i + 1}Level", 6, -1);
            }
        }
    }
}
