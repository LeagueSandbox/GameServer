using IntWarsSharp.Core.Logic;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic
{
    //TODO: rewrite using reflection
    class Config
    {
        public static Dictionary<string, PlayerConfig> players;
        public static GameConfig gameConfig;
        public static MapSpawns mapSpawns;
        public static string version = "Version 4.20.0.315 [PUBLIC]";

        public static void LoadConfig()
        {
            players = new Dictionary<string, PlayerConfig>();
            var script = new LuaScript();

            script.loadScript(System.IO.Path.Combine(Program.ExecutingDirectory, "lua", "config.lua"));

            var playerList = script.getTableDictionary("players");

            for (int i = 1; i < 12; i++)
            {
                var playerIndex = "player" + i;
                if (!playerList.ContainsKey(playerIndex))
                    continue;
                var player = playerList[playerIndex] as LuaTable;
                if (player == null)
                    continue;

                var config = new PlayerConfig();

                foreach (var p in script.getTableDictionary(player))
                {
                    switch (p.Key.ToString().ToLower())
                    {
                        case "rank":
                            config.rank = p.Value.ToString();
                            break;
                        case "name":
                            config.name = p.Value.ToString();
                            break;
                        case "champion":
                            config.champion = p.Value.ToString();
                            break;
                        case "team":
                            config.team = p.Value.ToString();
                            break;
                        case "skin":
                            config.skin = short.Parse(p.Value.ToString());
                            break;
                        case "summoner1":
                            config.summoner1 = p.Value.ToString();
                            break;
                        case "summoner2":
                            config.summoner2 = p.Value.ToString();
                            break;
                        case "ribbon":
                            config.ribbon = short.Parse(p.Value.ToString());
                            break;
                        case "icon":
                            config.icon = int.Parse(p.Value.ToString());
                            break;
                        default:
                            Logger.LogCoreInfo("Unknown player config " + p.Key.ToString());
                            break;
                    }
                }
                players.Add(playerIndex, config);
            }

            gameConfig = new GameConfig();
            var game = script.getTableDictionary("game");
            foreach (var g in game)
            {
                switch (g.Key.ToString().ToLower())
                {
                    case "map":
                        gameConfig.map = g.Value.ToString();
                        break;
                    default:
                        Logger.LogCoreInfo("Unknown game config " + g.Key.ToString());
                        break;
                }
            }

            mapSpawns = new MapSpawns();
            script.loadScript(System.IO.Path.Combine(Program.ExecutingDirectory, "lua", "maps", "map" + gameConfig.map + ".lua"));
            var teams = script.getTableDictionary("spawnpoints");
            foreach (var team in teams)
            {
                var teamColor = team.Key.ToString().ToLower();
                var numOfPlayers = team.Value as LuaTable;
                if (numOfPlayers == null)
                    continue;

                foreach (var num in script.getTableDictionary(numOfPlayers))
                {
                    var number = int.Parse(num.Key.ToString());
                    var spawns = num.Value as LuaTable;
                    if (spawns == null)
                        continue;
                    var playerSpawns = new PlayerSpawns();

                    foreach (var spawn in script.getTableDictionary(spawns))
                    {
                        var spawnName = spawn.Key;
                        var spawnPoint = int.Parse(spawn.Value.ToString());
                        switch (spawnName.ToString().ToLower())
                        {
                            case "player1x":
                                playerSpawns.player1X = spawnPoint;
                                break;
                            case "player1y":
                                playerSpawns.player1Y = spawnPoint;
                                break;
                            case "player2x":
                                playerSpawns.player2X = spawnPoint;
                                break;
                            case "player2y":
                                playerSpawns.player2Y = spawnPoint;
                                break;
                            case "player3x":
                                playerSpawns.player3X = spawnPoint;
                                break;
                            case "player3y":
                                playerSpawns.player3Y = spawnPoint;
                                break;
                            case "player4x":
                                playerSpawns.player4X = spawnPoint;
                                break;
                            case "player4y":
                                playerSpawns.player4Y = spawnPoint;
                                break;
                            case "player5x":
                                playerSpawns.player5X = spawnPoint;
                                break;
                            case "player5y":
                                playerSpawns.player5Y = spawnPoint;
                                break;
                            case "player6x":
                                playerSpawns.player6X = spawnPoint;
                                break;
                            case "player6y":
                                playerSpawns.player6Y = spawnPoint;
                                break;
                        }
                    }

                    switch (teamColor)
                    {
                        case "blue":
                            mapSpawns.blue.Add(number, playerSpawns);
                            break;
                        case "purple":
                            mapSpawns.purple.Add(number, playerSpawns);
                            break;
                    }
                }
            }
        }
    }

    internal class MapSpawns
    {
        public Dictionary<int, PlayerSpawns> blue = new Dictionary<int, PlayerSpawns>();
        public Dictionary<int, PlayerSpawns> purple = new Dictionary<int, PlayerSpawns>();
    }

    internal class PlayerSpawns
    {
        public int player1X { get; set; }
        public int player1Y { get; set; }
        public int player2X { get; set; }
        public int player2Y { get; set; }
        public int player3X { get; set; }
        public int player3Y { get; set; }
        public int player4X { get; set; }
        public int player4Y { get; set; }
        public int player5X { get; set; }
        public int player5Y { get; set; }
        public int player6X { get; set; }
        public int player6Y { get; set; }

        internal int getXForPlayer(int playerId)
        {
            switch (playerId)
            {
                case 1:
                    return player1X;
                case 2:
                    return player2X;
                case 3:
                    return player3X;
                case 4:
                    return player4X;
                case 5:
                    return player5X;
                case 6:
                    return player6X;
            }
            return 0;
        }

        internal int getYForPlayer(int playerId)
        {
            switch (playerId)
            {
                case 1:
                    return player1Y;
                case 2:
                    return player2Y;
                case 3:
                    return player3Y;
                case 4:
                    return player4Y;
                case 5:
                    return player5Y;
                case 6:
                    return player6Y;
            }
            return 0;
        }
    }

    internal class GameConfig
    {
        public string map { get; set; }
    }


    internal class PlayerConfig
    {
        public string rank { get; set; }
        public string name { get; set; }
        public string champion { get; set; }
        public string team { get; set; }
        public short skin { get; set; }
        public string summoner1 { get; set; }
        public string summoner2 { get; set; }
        public short ribbon { get; set; }
        public int icon { get; set; }
    }
}
