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
    internal class Server : IDisposable
    {
        private Dictionary<ulong, string> _blowfishKeys;
        private string _serverVersion = "0.2.0";
        private readonly ILog _logger;
        private Game _game;
        private Config _config;
        private ushort _serverPort { get; }

        public Server(Game game, ushort port, string configJson)
        {
            _logger = LoggerProvider.GetLogger();
            _game = game;
            _serverPort = port;
            _config = Config.LoadFromJson(game, configJson);

            _blowfishKeys = new Dictionary<ulong, string>();
            foreach (var player in _config.Players)
                _blowfishKeys.Add(player.Value.PlayerID, player.Value.BlowfishKey);
        }

        public void Start()
        {
            var build = $"League Sandbox Build {ServerContext.BuildDateString}";
            var packetServer = new PacketServer();

            Console.Title = build;

            _logger.Debug(build);
            _logger.Debug($"Yorick {_serverVersion}");
            _logger.Info($"Game started on port: {_serverPort}");

            packetServer.InitServer(_serverPort, _blowfishKeys, _game, _game.RequestHandler, _game.ResponseHandler);
            _game.Initialize(_config, packetServer);
        }

        public void StartNetworkLoop()
        {
            _game.GameLoop();
        }

        public void Dispose()
        {
            // PathNode.DestroyTable();
        }
    }
}
