using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Content;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer
{
    public class Config
    {
        public Dictionary<string, PlayerConfig> Players { get; private set; }
        public GameConfig GameConfig { get; private set; }
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

            // Read global damage text setting
            IsDamageTextGlobal = (bool)gameInfo.SelectToken("IS_DAMAGE_TEXT_GLOBAL");

            // Load items
            game.ItemManager.AddItems(ItemContentCollection.LoadItemsFrom(
                // todo: remove this hardcoded path with content pipeline refactor
                $"{ContentPath}/LeagueSandbox-Default/Items"
            ));

            // Read the game configuration
            var gameToken = data.SelectToken("game");
            GameConfig = new GameConfig(gameToken);

            // Read spawns info
            ContentManager = ContentManager.LoadGameMode(game, GameConfig.GameMode, ContentPath);
            var mapPath = ContentManager.GetMapDataPath(GameConfig.Map);
            var mapData = JObject.Parse(File.ReadAllText(mapPath));
            var spawns = mapData.SelectToken("spawns");

            MapSpawns = new MapSpawns();
            foreach (JProperty teamSpawn in spawns)
            {
                var team = teamSpawn.Name;
                var spawnsByPlayerCount = (JArray)teamSpawn.Value;
                for (var i = 0; i < spawnsByPlayerCount.Count; i++)
                {
                    var playerSpawns = new PlayerSpawns((JArray)spawnsByPlayerCount[i]);
                    MapSpawns.SetSpawns(team, playerSpawns, i);
                }
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
        public string GameMode => (string)_gameData.SelectToken("gameMode");

        private JToken _gameData;

        public GameConfig(JToken gameData)
        {
            _gameData = gameData;
        }
    }


    public class PlayerConfig
    {
        public string Rank => (string)_playerData.SelectToken("rank");
        public string Name => (string)_playerData.SelectToken("name");
        public string Champion => (string)_playerData.SelectToken("champion");
        public string Team => (string)_playerData.SelectToken("team");
        public short Skin => (short)_playerData.SelectToken("skin");
        public string Summoner1 => (string)_playerData.SelectToken("summoner1");
        public string Summoner2 => (string)_playerData.SelectToken("summoner2");
        public short Ribbon => (short)_playerData.SelectToken("ribbon");
        public int Icon => (int)_playerData.SelectToken("icon");
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
