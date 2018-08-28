using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameServerApp.Configs
{
    public class GameServerAppConfig
    {
        public string GamePath { get; set; } = "C:\\LeagueSandbox\\League_Sandbox_Client";
        public string GameMode { get; set; } = "LeagueSandbox-Default";
        public string ContentPath { get; set; } = "../../../..";
        public string Rank { get; set; } = "Diamond";
        public string Champion { get; set; } = "Ezreal";
        public string Name { get; set; } = "Giggle Bear";
        public int Map { get; set; } = 1;
        public string Team { get; set; } = "Blue";
        public int Icon { get; set; } = 0;
        public int Skin { get; set; } = 0;
        public int Ribbon { get; set; } = 2;
        public string Summoner1 { get; set; } = "SummonerHeal";
        public string Summoner2 { get; set; } = "SummonerFlash";
        public bool EnableCooldowns { get; set; } = false;
        public bool EnableSpawnMinions { get; set; } = true;
        public bool EnableManaCosts { get; set; } = false;
        public bool EnableCheats { get; set; } = true;
        public bool DisplayDebugLogs { get; set; } = false;
        public bool DisplayInfoLogs { get; set; } = true;
        public bool DisplayWarningLogs { get; set; } = true;
        public bool DisplayErrorLogs { get; set; } = true;
        public bool AutoStartServerOnLaunch { get; set; } = true;
        public bool AutoStartGameWithServer { get; set; } = true;

        public GameServerAppConfig()
        {
        }

        public void LoadFromJson(string json)
        {
            LoadConfig(json);
        }

        public void LoadFromFile(string path)
        {
            if (File.Exists(path))
            {
                LoadConfig(File.ReadAllText(path));
            } else
            {
                SaveToFile(path);
            }
        }

        public void SaveToFile(string path)
        {
            SaveConfig(path);
        }

        private void LoadConfig(string json)
        {
            var data = JObject.Parse(json);

            GamePath = (string)data.SelectToken("GamePath");
            GameMode = (string)data.SelectToken("GameMode");
            ContentPath = (string)data.SelectToken("ContentPath");
            Rank = (string)data.SelectToken("Rank");
            Champion = (string)data.SelectToken("Champion");
            Name = (string)data.SelectToken("Name");
            Map = (int)data.SelectToken("Map");
            Team = (string)data.SelectToken("Team");
            Icon = (int)data.SelectToken("Icon");
            Skin = (int)data.SelectToken("Skin");
            Ribbon = (int)data.SelectToken("Ribbon");
            Summoner1 = (string)data.SelectToken("Summoner1");
            Summoner2 = (string)data.SelectToken("Summoner2");
            EnableCooldowns = (bool)data.SelectToken("EnableCooldowns");
            EnableSpawnMinions = (bool)data.SelectToken("EnableSpawnMinions");
            EnableManaCosts = (bool)data.SelectToken("EnableManaCosts");
            EnableCheats = (bool)data.SelectToken("EnableCheats");
            DisplayInfoLogs = (bool)data.SelectToken("DisplayInfoLogs");
            DisplayDebugLogs = (bool)data.SelectToken("DisplayDebugLogs");
            DisplayWarningLogs = (bool)data.SelectToken("DisplayWarningLogs");
            DisplayErrorLogs = (bool)data.SelectToken("DisplayErrorLogs");
            AutoStartServerOnLaunch = (bool)data.SelectToken("AutoStartServerOnLaunch");
            AutoStartGameWithServer = (bool)data.SelectToken("AutoStartGameWithServer");
        }

        private void SaveConfig(string path)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer,
                    new
                    {
                        GamePath,
                        GameMode,
                        ContentPath,
                        Rank,
                        Champion,
                        Name,
                        Map,
                        Team,
                        Icon,
                        Skin,
                        Ribbon,
                        Summoner1,
                        Summoner2,
                        EnableCooldowns,
                        EnableSpawnMinions,
                        EnableManaCosts,
                        EnableCheats,
                        DisplayInfoLogs,
                        DisplayDebugLogs,
                        DisplayWarningLogs,
                        DisplayErrorLogs,
                        AutoStartServerOnLaunch,
                        AutoStartGameWithServer
                    }
                );
            }
        }
    }
}