using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using LeagueSandbox.GameServer.Content;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer
{
    public class Config
    {
        public Dictionary<string, PlayerConfig> Players { get; set; }
        public GameConfig GameConfig { get; set; }
        public const string VERSION_STRING = "Version 4.20.0.315 [PUBLIC]";
        public static readonly Version VERSION = new Version(4, 20, 0, 315);

        public bool CooldownsEnabled { get; set; }
        public bool ManaCostsEnabled { get; set; }
        public bool ChatCheatsEnabled { get; set; }
        public bool MinionSpawnsEnabled { get; set; }
        public string ContentPath { get; set; }

        public Config()
        {
        }

        public static Config LoadFromJson(string json)
        {
            var result = new Config();
            result.LoadConfig(json);
            return result;
        }

        public static Config LoadFromFile(string path)
        {
            var result = new Config();
            result.LoadConfig(File.ReadAllText(path));
            return result;
        }

        private void LoadConfig(string json)
        {
            Players = new Dictionary<string, PlayerConfig>();

            var data = JObject.Parse(json);

            // Read the player configuration
            var playerConfigurations = data.SelectToken("players");
            foreach (var player in playerConfigurations)
            {
                var playerConfig = new PlayerConfig(player);
                var playerNum = Players.Count + 1;
                Players.Add($"player{playerNum}", playerConfig);
            }

            // Read cost/cd info
            var gameInfo = data.SelectToken("gameInfo");
            CooldownsEnabled = (bool)gameInfo.SelectToken("COOLDOWNS_ENABLED");
            ManaCostsEnabled = (bool)gameInfo.SelectToken("MANACOSTS_ENABLED");

            // Read if chat commands are enabled
            ChatCheatsEnabled = (bool)gameInfo.SelectToken("CHEATS_ENABLED");

            // Read if minion spawns are enabled
            MinionSpawnsEnabled = (bool)gameInfo.SelectToken("MINION_SPAWNS_ENABLED");

            // Read where the content is
            ContentPath = (string)gameInfo.SelectToken("CONTENT_PATH");

            // Read the game configuration
            var gameToken = data.SelectToken("game");
            GameConfig = new GameConfig(gameToken);

        }
    }

    public class MapSpawns
    {
        public Dictionary<int, PlayerSpawns> Blue = new Dictionary<int, PlayerSpawns>();
        public Dictionary<int, PlayerSpawns> Purple = new Dictionary<int, PlayerSpawns>();

        public void SetSpawns(string team, PlayerSpawns spawns, int playerCount)
        {
            if (team.ToLower().Equals("blue"))
            {
                Blue[playerCount] = spawns;
            }
            else
            {
                Purple[playerCount] = spawns;
            }
        }
    }

    public class PlayerSpawns
    {
        private JArray _spawns;

        public PlayerSpawns(JArray spawns)
        {
            _spawns = spawns;
        }

        internal Vector2 GetCoordsForPlayer(int playerId)
        {
            return new Vector2((int)((JArray)_spawns[playerId])[0], (int)((JArray)_spawns[playerId])[1]);
        }
    }

    public class GameConfig
    {
        public int Map;
        public string GameMode;

        public GameConfig(JToken gameData) : this((int)gameData.SelectToken("map"), (string)gameData.SelectToken("gameMode"))
        {
        }

        public GameConfig(int map, string gameMode)
        {
            Map = map;
            GameMode = gameMode;
        }
    }


    public class PlayerConfig
    {
        public string Rank;
        public string Name;
        public string Champion;
        public string Team;
        public short Skin;
        public string Summoner1;
        public string Summoner2;
        public short Ribbon;
        public int Icon;
        public RuneCollection Runes = new RuneCollection();

        public PlayerConfig()
        {
        }

        public PlayerConfig(JToken playerData)
        {
            Rank = (string)playerData.SelectToken("rank");
            Name = (string)playerData.SelectToken("name");
            Champion = (string)playerData.SelectToken("champion");
            Team = (string)playerData.SelectToken("team");
            Skin = (short)playerData.SelectToken("skin");
            Summoner1 = (string)playerData.SelectToken("summoner1");
            Summoner2 = (string)playerData.SelectToken("summoner2");
            Ribbon = (short)playerData.SelectToken("ribbon");
            Icon = (int)playerData.SelectToken("icon");

            try
            {
                var runes = playerData.SelectToken("runes");

                foreach (JProperty runeCategory in runes)
                {
                    Runes.Add(Convert.ToInt32(runeCategory.Name), Convert.ToInt32(runeCategory.Value));
                }
            }
            catch (Exception)
            {
                // no runes set in config
            }
        }
    }
}