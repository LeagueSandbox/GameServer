using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using GameServerCore.Domain;
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
            IsDamageTextGlobal = (bool) gameInfo.SelectToken("IS_DAMAGE_TEXT_GLOBAL");

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
        public Dictionary<string, MapObject> MapObjects { get; private set; }

        public MapData(int mapId)
        {
            Id = mapId;
            MapObjects = new Dictionary<string, MapObject>();
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
