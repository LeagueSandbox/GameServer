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
        public Dictionary<string, PlayerConfig> Players { get; private set; }
        public GameConfig GameConfig { get; private set; }
        public MapData MapData { get; private set; }
        public MapSpawns MapSpawns { get; private set; }
        public ContentManager ContentManager { get; private set; }
        public const string VERSION_STRING = "Version 4.20.0.315 [PUBLIC]";
        public static readonly Version VERSION = new Version(4, 20, 0, 315);

        public bool CooldownsEnabled { get; private set; }
        public bool ManaCostsEnabled { get; private set; }
        public bool ChatCheatsEnabled { get; private set; }
        public bool MinionSpawnsEnabled { get; private set; }
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
            Players = new Dictionary<string, PlayerConfig>();

            var data = JObject.Parse(json);

            // Read the player configuration
            var playerConfigurations = data.SelectToken("players");
            foreach (var player in playerConfigurations)
            {
                var playerConfig = new PlayerConfig(player);
                Players.Add($"player{playerConfig.PlayerID}", playerConfig);
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
            MapSpawns = ContentManager.GetMapSpawns(GameConfig.Map);
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
    }

    public class MapData
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

        public class MapObject
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
                else if (Name.Contains("Barracks"))
                {
                    // Inhibitors are dampeners for the enemy Nexus.
                    type = GameObjectTypes.ObjAnimated_BarracksDampener;
                }
                else if (Name.Contains("Turret"))
                {
                    type = GameObjectTypes.ObjAIBase_Turret;
                }

                return type;
            }

            public TeamId GetTeamID()
            {
                var team = TeamId.TEAM_NEUTRAL;

                if (Name.Contains("T1") || Name.Contains("Order"))
                {
                    team = TeamId.TEAM_BLUE;
                }
                else if (Name.Contains("T2") || Name.Contains("Chaos"))
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
                else if (Name.Contains("_C"))
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
        public int Map => (int)_gameData.SelectToken("map");
        public string DataPackage => (string)_gameData.SelectToken("dataPackage");

        private JToken _gameData;

        public GameConfig(JToken gameData)
        {
            _gameData = gameData;
        }
    }


    public class PlayerConfig
    {
        public long PlayerID => (long)_playerData.SelectToken("playerId");
        public string Rank => (string)_playerData.SelectToken("rank");
        public string Name => (string)_playerData.SelectToken("name");
        public string Champion => (string)_playerData.SelectToken("champion");
        public string Team => (string)_playerData.SelectToken("team");
        public short Skin => (short)_playerData.SelectToken("skin");
        public string Summoner1 => (string)_playerData.SelectToken("summoner1");
        public string Summoner2 => (string)_playerData.SelectToken("summoner2");
        public short Ribbon => (short)_playerData.SelectToken("ribbon");
        public int Icon => (int)_playerData.SelectToken("icon");
        public string BlowfishKey => (string)_playerData.SelectToken("blowfishKey");
        public IRuneCollection Runes { get; }

        private JToken _playerData;

        public PlayerConfig(JToken playerData)
        {
            _playerData = playerData;
            try
            {
                var runes = _playerData.SelectToken("runes");
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
    }
}
