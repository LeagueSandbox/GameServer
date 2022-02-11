using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using GameServerCore.Domain;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
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
        public MapData MapData { get; private set; }
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
            Players = new Dictionary<string, IPlayerConfig>();

            var data = JObject.Parse(json);

            // Read the player configuration
            var playerConfigurations = data.SelectToken("players");
            foreach (var player in playerConfigurations)
            {
                var playerConfig = new PlayerConfig(player);
                Players.Add($"player{playerConfig.PlayerID}", playerConfig);
            }

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

            // Read data & spawns info
            MapData = ContentManager.GetMapData(GameConfig.Map);
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
    public Dictionary<string, IMapObject> MapObjects { get; private set; }
    /// <summary>
    /// Collection of MapObjects which represent lane minion spawn positions.
    /// Not present within the room file, therefor it is split into its own collection.
    /// </summary>
    public Dictionary<string, IMapObject> SpawnBarracks { get; private set; }
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
        MapObjects = new Dictionary<string, IMapObject>();
        SpawnBarracks = new Dictionary<string, IMapObject>();
        ExpCurve = new List<float>();
        DeathTimes = new List<float>();
        StatsProgression = new List<float>();
    }

    public class MapObject : IMapObject
    {
        public string Name { get; private set; }
        public Vector3 CentralPoint { get; private set; }
        public int ParentMapId { get; private set; }

        public MapObject(string name, Vector3 point, int id)
        {
            Name = name;
            CentralPoint = point;
            ParentMapId = id;
        }

        public GameObjectTypes GetGameObjectType()
        {
            GameObjectTypes type = 0;

            if (Name.Contains("LevelProp"))
            {
                type = GameObjectTypes.LevelProp;
            }
            else if (Name.Contains("HQ"))
            {
                type = GameObjectTypes.ObjAnimated_HQ;
            }
            else if (Name.Contains("Barracks_T"))
            {
                // Inhibitors are dampeners for the enemy Nexus.
                type = GameObjectTypes.ObjAnimated_BarracksDampener;
            }
            else if (Name.Contains("Turret"))
            {
                type = GameObjectTypes.ObjAIBase_Turret;
            }
            else if (Name.Contains("__Spawn"))
            {
                type = GameObjectTypes.ObjBuilding_SpawnPoint;
            }
            else if (Name.Contains("__NAV"))
            {
                type = GameObjectTypes.ObjBuilding_NavPoint;
            }
            else if (Name.Contains("Info_Point"))
            {
                type = GameObjectTypes.InfoPoint;
            }
            else if (Name.Contains("Shop"))
            {
                type = GameObjectTypes.ObjBuilding_Shop;
            }
            return type;
        }

        public TeamId GetTeamID()
        {
            var team = TeamId.TEAM_NEUTRAL;

            if (Name.Contains("T1") || Name.ToLower().Contains("order"))
            {
                team = TeamId.TEAM_BLUE;
            }
            else if (Name.Contains("T2") || Name.ToLower().Contains("chaos"))
            {
                team = TeamId.TEAM_PURPLE;
            }

            return team;
        }

        public TeamId GetOpposingTeamID()
        {
            var team = TeamId.TEAM_NEUTRAL;

            if (Name.Contains("T1") || Name.Contains("Order"))
            {
                team = TeamId.TEAM_PURPLE;
            }
            else if (Name.Contains("T2") || Name.Contains("Chaos"))
            {
                team = TeamId.TEAM_BLUE;
            }

            return team;
        }

        public string GetTeamName()
        {
            string teamName = "";
            if (GetTeamID() == TeamId.TEAM_BLUE)
            {
                teamName = "Order";
            }
            // Chaos and Neutral
            else
            {
                teamName = "Chaos";
            }

            return teamName;
        }

        public LaneID GetLaneID()
        {
            var laneId = LaneID.NONE;

            if (Name.Contains("_L"))
            {
                laneId = LaneID.TOP;
            }
            //Using just _C would cause files with "_Chaos" to be mistakenly assigned as MidLane
            else if (Name.Contains("_C0") || Name.Contains("_C1") || Name.Contains("_C_"))
            {
                laneId = LaneID.MIDDLE;
            }
            else if (Name.Contains("_R"))
            {
                laneId = LaneID.BOTTOM;
            }

            return laneId;
        }

        public LaneID GetSpawnBarrackLaneID()
        {
            var laneId = LaneID.NONE;

            if (Name.Contains("__L"))
            {
                laneId = LaneID.TOP;
            }
            else if (Name.Contains("__C"))
            {
                laneId = LaneID.MIDDLE;
            }
            else if (Name.Contains("__R"))
            {
                laneId = LaneID.BOTTOM;
            }

            return laneId;
        }

        public int ParseIndex()
        {
            int index = -1;

            if (GetGameObjectType() == 0)
            {
                return index;
            }

            var underscoreIndices = new List<int>();

            // While there are underscores, it loops,
            for (int i = Name.IndexOf('_'); i > -1; i = Name.IndexOf('_', i + 1))
            {
                // and ends when i = -1 (no underscore found).
                underscoreIndices.Add(i);
            }

            // If the above failed to find any underscores or the underscore is the last character in the string.
            if (underscoreIndices.Count == 0 || underscoreIndices.Last() == underscoreIndices.Count)
            {
                return index;
            }

            // Otherwise, we make a new string which starts at the last underscore (+1 character to the right),
            string startString = Name.Substring(underscoreIndices.Last() + 1);

            // and we check it for an index.
            try
            {
                index = int.Parse(startString);
            }
            catch (FormatException)
            {
                return index;
            }

            return index;
        }
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

    public PlayerConfig(JToken playerData)
    {
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

        try
        {
            var runes = playerData.SelectToken("runes");
            Runes = new RuneCollection();

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

    public PlayerConfig(string name, string champion, long playerId = -1, string rank = "", string team = "BLUE", short skin = 0, string summoner1 = "SummonerHeal", string summoner2 = "SummonerFlash", short ribbon = 0, int icon = 0, string blowfishKey = "")
    {
        PlayerID = playerId;
        Rank = rank;
        Name = name;
        Champion = champion;
        Team = team;
        Skin = skin;
        Summoner1 = summoner1;
        Summoner2 = summoner2;
        Ribbon = ribbon;
        Icon = icon;
        BlowfishKey = blowfishKey;

        Runes = new RuneCollection();
    }
}
