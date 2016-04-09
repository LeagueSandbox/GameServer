using LeagueSandbox.GameServer.Core.Logic;
using Newtonsoft.Json.Linq;
using NLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic
{
    class Config
    {
        public static Dictionary<string, PlayerConfig> players;
        public static GameConfig gameConfig;
        public static MapSpawns mapSpawns;
        public static ContentManager contentManager;
        public static string version = "Version 4.20.0.315 [PUBLIC]";

        public static void LoadConfig(string path)
        {
            players = new Dictionary<string, PlayerConfig>();

            var data = JObject.Parse(File.ReadAllText(path));

            // Read the player configuration
            var playerConfigurations = data.SelectToken("players");
            foreach(var player in playerConfigurations)
            {
                var playerConfig = new PlayerConfig(player);
                players.Add(string.Format("player{0}", players.Count + 1), playerConfig);
            }

            // Read the game configuration
            var game = data.SelectToken("game");
            gameConfig = new GameConfig(game);

            // Read spawns info
            contentManager = ContentManager.LoadGameMode(gameConfig.gameMode);
            var mapPath = contentManager.GetMapDataPath(gameConfig.map);
            var mapData = JObject.Parse(File.ReadAllText(mapPath));
            var spawns = mapData.SelectToken("spawns");

            mapSpawns = new MapSpawns();
            foreach(JProperty teamSpawn in spawns)
            {
                var team = teamSpawn.Name;
                var spawnsByPlayerCount = (JArray)teamSpawn.Value;
                for(var i = 0; i < spawnsByPlayerCount.Count; i++)
                {
                    var playerSpawns = new PlayerSpawns((JArray)spawnsByPlayerCount[i]);
                    mapSpawns.SetSpawns(team, playerSpawns, i);
                }
            }
        }
    }

    internal class MapSpawns
    {
        public Dictionary<int, PlayerSpawns> blue = new Dictionary<int, PlayerSpawns>();
        public Dictionary<int, PlayerSpawns> purple = new Dictionary<int, PlayerSpawns>();

        public void SetSpawns(string team, PlayerSpawns spawns, int playerCount)
        {
            if(team.ToLower() == "blue")
            {
                blue[playerCount] = spawns;
            }
            else if(team.ToLower() == "purple")
            {
                purple[playerCount] = spawns;
            }
            else
            {
                throw new Exception("Invalid team");
            }
        }
    }

    internal class PlayerSpawns
    {
        private JArray _spawns;

        public PlayerSpawns(JArray spawns)
        {
            _spawns = spawns;
        }

        internal int getXForPlayer(int playerId)
        {
            return (int)((JArray)_spawns[playerId])[0];
        }

        internal int getYForPlayer(int playerId)
        {
            return (int)((JArray)_spawns[playerId])[0];
        }
    }

    internal class GameConfig
    {
        public int map { get { return (int)_gameData.SelectToken("map"); } }
        public string gameMode { get { return (string)_gameData.SelectToken("gameMode"); } }

        private JToken _gameData;

        public GameConfig(JToken gameData)
        {
            _gameData = gameData;
        }
    }


    internal class PlayerConfig
    {
        public string rank { get { return (string)_playerData.SelectToken("rank"); } }
        public string name { get { return (string)_playerData.SelectToken("name"); } }
        public string champion { get { return (string)_playerData.SelectToken("champion"); } }
        public string team { get { return (string)_playerData.SelectToken("team"); } }
        public short skin { get { return (short)_playerData.SelectToken("skin"); } }
        public string summoner1 { get { return (string)_playerData.SelectToken("summoner1"); } }
        public string summoner2 { get { return (string)_playerData.SelectToken("summoner2"); } }
        public short ribbon { get { return (short)_playerData.SelectToken("ribbon"); } }
        public int icon { get { return (int)_playerData.SelectToken("icon"); } }

        private JToken _playerData;

        public PlayerConfig(JToken playerData)
        {
            _playerData = playerData;
        }
    }
}
