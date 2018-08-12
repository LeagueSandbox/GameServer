using LeagueSandbox.GameServer.Core.Logic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Content
{
    public class PassiveData
    {
        public string PassiveNameStr { get; set; } = "";
        public string PassiveLuaName { get; set; } = "";
        public int[] PassiveLevels { get; set; } = { -1, -1, -1, -1, -1, -1 };
    }
    public class CharData
    {
        private Game _game = Program.ResolveDependency<Game>();
        private Logger _logger = Program.ResolveDependency<Logger>();

        public float BaseHP { get; private set;  } = 100.0f;
        public float BaseMP { get; private set;  } = 100.0f;
        public float BaseDamage { get; private set;  } = 10.0f;
        public float AttackRange { get; private set;  } = 100.0f;
        public int MoveSpeed { get; private set;  } = 100;
        public float Armor { get; private set;  } = 1.0f;
        public float SpellBlock { get; private set;  } = 0.0f;
        public float BaseStaticHPRegen { get; private set;  } = 0.30000001f;
        public float BaseStaticMPRegen { get; private set;  } = 0.30000001f;
        public float AttackDelayOffsetPercent { get; private set;  } = 0.0f;
        public float HPPerLevel { get; private set;  } = 10.0f;
        public float MPPerLevel { get; private set;  } = 10.0f;
        public float DamagePerLevel { get; private set;  } = 10.0f;
        public float ArmorPerLevel { get; private set;  } = 1.0f;
        public float SpellBlockPerLevel { get; private set;  } = 0.0f;
        public float HPRegenPerLevel { get; private set;  } = 0.0f;
        public float MPRegenPerLevel { get; private set;  } = 0.0f;
        public float AttackSpeedPerLevel { get; private set;  } = 0.0f;
        public bool IsMelee { get; private set; } = false; //Yes or no
        public float PathfindingCollisionRadius { get; private set;  } = -1.0f;
        public float GameplayCollisionRadius { get; private set;  } = 65.0f;

        public string[] SpellNames { get; private set; } = { "", "", "", "" };
        public int[] MaxLevels { get; private set; } = { 5, 5, 5, 3 };
        public int[][] SpellsUpLevels { get; private set; } =
        {
            new int[] {1, 3, 5, 7, 9, 99},
            new int[] {1, 3, 5, 7, 9, 99},
            new int[] {1, 3, 5, 7, 9, 99},
            new int[] {1, 3, 5, 7, 9, 99}
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
            ContentFile file = new ContentFile();
            try
            {
                var path = _game.Config.ContentManager.GetUnitStatPath(name);
                _logger.LogCoreInfo($"Loading {name}'s Stats  from path: {Path.GetFullPath(path)}!");
                var text = File.ReadAllText(Path.GetFullPath(path));
                file = JsonConvert.DeserializeObject<ContentFile>(text);
            }
            catch (ContentNotFoundException notfound)
            {
                _logger.LogCoreWarning($"Stats for {name} was not found: {notfound.Message}");
                return;
            }

            BaseHP = file.GetFloat("Data", "BaseHP", BaseHP);
            BaseMP = file.GetFloat("Data", "BaseMP", BaseMP);
            BaseDamage = file.GetFloat("Data", "BaseDamage", BaseDamage);
            AttackRange = file.GetFloat("Data", "AttackRange", AttackRange);
            MoveSpeed = file.GetInt("Data", "MoveSpeed", MoveSpeed);
            Armor = file.GetFloat("Data", "Armor", Armor);
            SpellBlock = file.GetFloat("Data", "SpellBlock", SpellBlock);
            BaseStaticHPRegen = file.GetFloat("Data", "BaseStaticHPRegen", BaseStaticHPRegen);
            BaseStaticMPRegen = file.GetFloat("Data", "BaseStaticMPRegen", BaseStaticMPRegen);
            AttackDelayOffsetPercent = file.GetFloat("Data", "AttackDelayOffsetPercent", AttackDelayOffsetPercent);
            HPPerLevel = file.GetFloat("Data", "HPPerLevel", HPPerLevel);
            MPPerLevel = file.GetFloat("Data", "MPPerLevel", MPPerLevel);
            DamagePerLevel = file.GetFloat("Data", "DamagePerLevel", DamagePerLevel);
            ArmorPerLevel = file.GetFloat("Data", "ArmorPerLevel", ArmorPerLevel);
            SpellBlockPerLevel = file.GetFloat("Data", "SpellBlockPerLevel", SpellBlockPerLevel);
            HPRegenPerLevel = file.GetFloat("Data", "HPRegenPerLevel", HPRegenPerLevel);
            MPRegenPerLevel = file.GetFloat("Data", "MPRegenPerLevel", MPRegenPerLevel);
            AttackSpeedPerLevel = file.GetFloat("Data", "AttackSpeedPerLevel", AttackSpeedPerLevel);
            IsMelee = file.GetString("Data", "IsMelee", IsMelee ? "Yes" : "No").Equals("yes");
            PathfindingCollisionRadius = file.GetFloat("Data", "PathfindingCollisionRadius", PathfindingCollisionRadius);
            GameplayCollisionRadius = file.GetFloat("Data", "GameplayCollisionRadius", GameplayCollisionRadius);

            for (int i = 0; i < 4; i++)
            {
                SpellNames[i] = file.GetString("Data", $"Spell{i + 1}", SpellNames[i]);
                
            }
            for (int i = 0; i < 4; i++)
            {
                SpellsUpLevels[i] = file.GetIntArray("Data", $"SpellsUpLevels{i+1}", SpellsUpLevels[i]);
            }
            MaxLevels = file.GetIntArray("Data", "MaxLevels", MaxLevels);
            for (int i = 0; i < 8; i++)
            {
                ExtraSpells[i] = file.GetString("Data", $"ExtraSpell{i + 1}", ExtraSpells[i]);
            }
            for (int i = 0; i < 6; i++)
            {
                Passives[i].PassiveNameStr = file.GetString("Data", $"Passive{i + 1}Name", Passives[i].PassiveNameStr);
                Passives[i].PassiveLuaName = file.GetString("Data", $"Passive{i + 1}LuaName", Passives[i].PassiveLuaName);
                Passives[i].PassiveLevels = file.GetMultiInt("Data", $"Passive{i + 1}Level", 6, -1);
            }
        }
    }
}
