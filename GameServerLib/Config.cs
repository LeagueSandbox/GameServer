using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using GameServerCore.Domain;
using GameServerCore.Enums;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.Inventory;
using LeagueSandbox.GameServer.Logging;
using log4net;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer
{
    /// <summary>
    /// Class that contains basic game information which is used to decide how the game will function after starting, such as players, their spawns,
    /// the packages which control the functionality of their champions/abilities, and lastly whether basic game mechanics such as 
    /// cooldowns/mana costs/minion spawns should be enabled/disabled.
    /// </summary>
    public class Config
    {
        public Dictionary<string, IPlayerConfig> Players { get; private set; }
        public GameConfig GameConfig { get; private set; }
        public ContentManager ContentManager { get; private set; }
        public FeatureFlags GameFeatures { get; private set; }
        public const string VERSION_STRING = "Version 4.20.0.315 [PUBLIC]";
        public static readonly Version VERSION = new Version(4, 20, 0, 315);

        public bool ChatCheatsEnabled { get; private set; }
        public string ContentPath { get; private set; }
        public bool IsDamageTextGlobal { get; private set; }

        private Config()
        {
        }

        public static Config LoadFromJson(Game game, string json)
        {
            var result = new Config();
            result.LoadConfig(game, json);
            return result;
        }

        public static Config LoadFromFile(Game game, string path)
        {
            var result = new Config();
            result.LoadConfig(game, File.ReadAllText(path));
            return result;
        }

        private void LoadConfig(Game game, string json)
        {
            var data = JObject.Parse(json);

            var gameInfo = data.SelectToken("gameInfo");
            SetGameFeatures(FeatureFlags.EnableCooldowns, (bool)gameInfo.SelectToken("COOLDOWNS_ENABLED"));
            SetGameFeatures(FeatureFlags.EnableManaCosts, (bool)gameInfo.SelectToken("MANACOSTS_ENABLED"));
            SetGameFeatures(FeatureFlags.EnableLaneMinions, (bool)gameInfo.SelectToken("MINION_SPAWNS_ENABLED"));

            // Read if chat commands are enabled
            ChatCheatsEnabled = (bool)gameInfo.SelectToken("CHEATS_ENABLED");

            // Read where the content is
            ContentPath = (string)gameInfo.SelectToken("CONTENT_PATH");

            // Evaluate if content path is correct, if not try to path traversal to find it
            if (!Directory.Exists(ContentPath))
            {
                ContentPath = GetContentPath();
            }

            // Read global damage text setting
            IsDamageTextGlobal = (bool)gameInfo.SelectToken("IS_DAMAGE_TEXT_GLOBAL");

            // Read the game configuration
            var gameToken = data.SelectToken("game");
            GameConfig = new GameConfig(gameToken);

            // Load data package
            ContentManager = ContentManager.LoadDataPackage(game, GameConfig.DataPackage, ContentPath);

            Players = new Dictionary<string, IPlayerConfig>();

            // Read the player configuration
            var playerConfigurations = data.SelectToken("players");
            foreach (var player in playerConfigurations)
            {
                var playerConfig = new PlayerConfig(player, game);
                Players.Add($"player{playerConfig.PlayerID}", playerConfig);
            }
        }

        private string GetContentPath()
        {
            string result = null;

            var executionDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = new DirectoryInfo(executionDirectory ?? Directory.GetCurrentDirectory());

            while (result == null)
            {
                if (path == null)
                {
                    break;
                }

                var directory = path.GetDirectories().Where(c => c.Name.Equals("Content")).ToArray();

                if (directory.Length == 1)
                {
                    result = directory[0].FullName;
                }
                else
                {
                    path = path.Parent;
                }
            }

            return result;
        }

        public void SetGameFeatures(FeatureFlags flag, bool enabled)
        {
            // Toggle the flag on.
            if (enabled)
            {
                GameFeatures |= flag;
            }
            // Toggle off.
            else
            {
                GameFeatures &= ~flag;
            }
        }

        public Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> GetMapSpawns()
        {
            Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> toReturn = new Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>>();
            foreach (var rawInfo in ContentManager.GetMapSpawns(GameConfig.Map))
            {
                var team = TeamId.TEAM_BLUE;
                if (rawInfo.Key.ToLower().Equals("purple"))
                {
                    team = TeamId.TEAM_PURPLE;
                }

                for (int i = 0; i < rawInfo.Value.Count; i++)
                {
                    for (int j = 0; j < rawInfo.Value[i].Count(); j++)
                    {
                        if (toReturn.ContainsKey(team))
                        {
                            if (toReturn[team].ContainsKey(i + 1))
                            {
                                toReturn[team][i + 1].Add(j + 1, new Vector2((int)((JArray)rawInfo.Value[i][j])[0], (int)((JArray)rawInfo.Value[i][j])[1]));
                            }
                            else
                            {
                                toReturn[team].Add(rawInfo.Value[i].Count(), new Dictionary<int, Vector2>{
                                    { j + 1, new Vector2((int)((JArray)rawInfo.Value[i][j])[0], (int)((JArray)rawInfo.Value[i][j])[1]) } });
                            }
                        }
                        else
                        {
                            toReturn.Add(team, new Dictionary<int, Dictionary<int, Vector2>> { { rawInfo.Value[i].Count(), new Dictionary<int, Vector2> {
                                { j + 1, new Vector2((int)((JArray)rawInfo.Value[i][j])[0], (int)((JArray)rawInfo.Value[i][j])[1]) } } } });
                        }
                    }
                }
            }
            return toReturn;
        }
    }
}
public class MapData : IMapData
{
    public int Id { get; private set; }
    /// <summary>
    /// Collection of MapObjects present within a map's room file, with the key being the name present in the room file. Refer to <see cref="MapObject"/>.
    /// </summary>
    public Dictionary<string, MapObject> MapObjects { get; private set; }
    /// <summary>
    /// Collection of MapObjects which represent lane minion spawn positions.
    /// Not present within the room file, therefor it is split into its own collection.
    /// </summary>
    public Dictionary<string, MapObject> SpawnBarracks { get; private set; }
    /// <summary>
    /// Experience required to level, ordered from 2 and up.
    /// </summary>
    public List<float> ExpCurve { get; private set; }
    public float BaseExpMultiple { get; set; }
    public float LevelDifferenceExpMultiple { get; set; }
    public float MinimumExpMultiple { get; set; }
    /// <summary>
    /// Amount of time death should last depending on level.
    /// </summary>
    public List<float> DeathTimes { get; private set; }
    /// <summary>
    /// Potential progression of stats per-level of jungle monsters.
    /// </summary>
    /// TODO: Figure out what this is and how to implement it.
    public List<float> StatsProgression { get; private set; }

    public MapData(int mapId)
    {
        Id = mapId;
        MapObjects = new Dictionary<string, MapObject>();
        SpawnBarracks = new Dictionary<string, MapObject>();
        ExpCurve = new List<float>();
        DeathTimes = new List<float>();
        StatsProgression = new List<float>();
    }
}

public class GameConfig
{
    public int Map => (int)_gameData.SelectToken("map");
    public string GameMode => _gameData.SelectToken("gameMode").ToString().ToUpper().Replace(" ", string.Empty);
    public string DataPackage => (string)_gameData.SelectToken("dataPackage");

    private JToken _gameData;

    public GameConfig(JToken gameData)
    {
        _gameData = gameData;
    }
}

public class PlayerConfig : IPlayerConfig
{
    public long PlayerID { get; private set; }
    public string Rank { get; private set; }
    public string Name { get; private set; }
    public string Champion { get; private set; }
    public string Team { get; private set; }
    public short Skin { get; private set; }
    public string Summoner1 { get; private set; }
    public string Summoner2 { get; private set; }
    public short Ribbon { get; private set; }
    public int Icon { get; private set; }
    public string BlowfishKey { get; private set; }
    public IRuneCollection Runes { get; }
    public ITalentInventory Talents { get; }

    public PlayerConfig(JToken playerData, Game game)
    {
        ILog logger = LoggerProvider.GetLogger();

        PlayerID = (long)playerData.SelectToken("playerId");
        Rank = (string)playerData.SelectToken("rank");
        Name = (string)playerData.SelectToken("name");
        Champion = (string)playerData.SelectToken("champion");
        Team = (string)playerData.SelectToken("team");
        Skin = (short)playerData.SelectToken("skin");
        Summoner1 = (string)playerData.SelectToken("summoner1");
        Summoner2 = (string)playerData.SelectToken("summoner2");
        Ribbon = (short)playerData.SelectToken("ribbon");
        Icon = (int)playerData.SelectToken("icon");
        BlowfishKey = (string)playerData.SelectToken("blowfishKey");

        Runes = new RuneCollection();
        var runes = playerData.SelectToken("runes");
        if (runes != null)
        {
            foreach (JProperty runeCategory in runes)
            {
                Runes.Add(Convert.ToInt32(runeCategory.Name), Convert.ToInt32(runeCategory.Value));
            }
        }
        else
        {
            logger.Warn($"No runes found for player {PlayerID}!");
        }

        Talents = new TalentInventory();
        var talents = playerData.SelectToken("talents");
        if (talents != null)
        {
            foreach (JProperty talent in talents)
            {
                byte level = 1;
                try
                {
                    level = talent.Value.Value<byte>();
                }
                catch
                {
                    logger.Warn($"Invalid Talent Rank for Talent {talent.Name}! Please use ranks between 1 and {byte.MaxValue}! Defaulting to Rank 1...");
                }
                Talents.Add(talent.Name, level);
            }
        }
    }
}
