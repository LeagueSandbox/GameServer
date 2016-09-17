using LeagueSandbox.GameServer.Logic.Content;
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
    public class Config
    {
        public Dictionary<string, PlayerConfig> Players { get; private set; }
        public GameConfig GameConfig { get; private set; }
        public MapSpawns MapSpawns { get; private set; }
        public ContentManager ContentManager { get; private set; }
        public const string VERSION = "Version 4.20.0.315 [PUBLIC]";

        public bool CooldownsDisabled { get; protected set; }
        public bool ManaCostsDisabled { get; protected set; }

        public Config(string path)
        {
            LoadConfig(path);
        }

        private void LoadConfig(string path)
        {
            Players = new Dictionary<string, PlayerConfig>();

            var data = JObject.Parse(File.ReadAllText(path));

            // Read the player configuration
            var playerConfigurations = data.SelectToken("players");
            foreach (var player in playerConfigurations)
            {
                var playerConfig = new PlayerConfig(player);
                Players.Add(string.Format("player{0}", Players.Count + 1), playerConfig);
            }

            //Read cost/cd info
            var spellInfo = data.SelectToken("spellInfo");
            CooldownsDisabled = (bool)spellInfo.SelectToken("NO_COOLDOWN");
            ManaCostsDisabled = (bool)spellInfo.SelectToken("NO_MANACOST");

            // Read the game configuration
            var game = data.SelectToken("game");
            GameConfig = new GameConfig(game);

            // Read spawns info
            ContentManager = ContentManager.LoadGameMode(GameConfig.GameMode);
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
            if (team.ToLower() == "blue")
            {
                Blue[playerCount] = spawns;
            }
            else if (team.ToLower() == "purple")
            {
                Purple[playerCount] = spawns;
            }
            else
            {
                throw new Exception("Invalid team");
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

        internal int GetXForPlayer(int playerId)
        {
            return (int)((JArray)_spawns[playerId])[0];
        }

        internal int GetYForPlayer(int playerId)
        {
            return (int)((JArray)_spawns[playerId])[0];
        }
    }

    public class GameConfig
    {
        public int Map { get { return (int)_gameData.SelectToken("map"); } }
        public string GameMode { get { return (string)_gameData.SelectToken("gameMode"); } }

        private JToken _gameData;

        public GameConfig(JToken gameData)
        {
            _gameData = gameData;
        }
    }


    public class PlayerConfig
    {
        public string Rank { get { return (string)_playerData.SelectToken("rank"); } }
        public string Name { get { return (string)_playerData.SelectToken("name"); } }
        public string Champion { get { return (string)_playerData.SelectToken("champion"); } }
        public string Team { get { return (string)_playerData.SelectToken("team"); } }
        public short Skin { get { return (short)_playerData.SelectToken("skin"); } }
        public string Summoner1 { get { return (string)_playerData.SelectToken("summoner1"); } }
        public string Summoner2 { get { return (string)_playerData.SelectToken("summoner2"); } }
        public short Ribbon { get { return (short)_playerData.SelectToken("ribbon"); } }
        public int Icon { get { return (int)_playerData.SelectToken("icon"); } }
        public RuneCollection Runes { get { return _runeList; } }

        private JToken _playerData;
        private RuneCollection _runeList;

        public PlayerConfig(JToken playerData)
        {
            _playerData = playerData;
            try
            {
                var runes = _playerData.SelectToken("runes");
                _runeList = new RuneCollection();

                foreach (JProperty runeCategory in runes)
                {
                    _runeList.Add(Convert.ToInt32(runeCategory.Name), Convert.ToInt32(runeCategory.Value));
                }
            }
            catch (Exception)
            {
                // no runes set in config
            }
        }
    }
}
