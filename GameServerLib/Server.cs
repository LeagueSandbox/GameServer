using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions;
using LeagueSandbox.GameServer.Logging;
using log4net;
using PacketDefinitions420;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace LeagueSandbox.GameServer
{
    /// <summary>
    /// Class which controls the starting of the game and network loops.
    /// </summary>
    internal class Server : IDisposable
    {
        private Dictionary<ulong, string> _blowfishKeys;
        private string _serverVersion = "0.2.0";
        private readonly ILog _logger;
        private Game _game;
        private Config _config;
        private string _serverIp { get; }
        private ushort _serverPort { get; }

        /// <summary>
        /// Initialize base variables for future usage.
        /// </summary>
        public Server(Game game, string ipaddr, ushort port, string configJson)
        {
            _logger = LoggerProvider.GetLogger();
            _game = game;
            _serverIp = ipaddr;
            _serverPort = port;
            _config = Config.LoadFromJson(game, configJson);

            _blowfishKeys = new Dictionary<ulong, string>();
            foreach (var player in _config.Players)
                _blowfishKeys.Add(player.Value.PlayerID, player.Value.BlowfishKey);
        }

        /// <summary>
        /// Called upon the Program successfully initializing GameServerLauncher.
        /// </summary>
        public void Start()
        {
            var build = $"League Sandbox Build {ServerContext.BuildDateString}";
            var packetServer = new PacketServer();

            Console.Title = build;

            _logger.Debug(build);
            _logger.Debug($"Yorick {_serverVersion}");
            _logger.Info($"Game started on: {_serverIp}:{_serverPort}");

            packetServer.InitServer(_serverIp, _serverPort, _blowfishKeys, _game, _game.RequestHandler, _game.ResponseHandler);
            _game.Initialize(_config, packetServer);
        }

        /// <summary>
        /// Called after the Program has finished setting up the Server for players to join.
        /// </summary>
        public void StartNetworkLoop()
        {
            _game.GameLoop();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. Unused.
        /// </summary>
        public void Dispose()
        {
            // PathNode.DestroyTable();
        }
    }
}
