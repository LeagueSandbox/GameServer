using System;
using ENet;
using LeagueSandbox.GameServer.Logic;

namespace LeagueSandbox.GameServer
{
    internal class Server : IDisposable
    {
        private string _blowfishKey = "17BLOhi6KZsTtldTsizvHg==";
        private uint _serverHost = Address.IPv4HostAny;
        private string _serverVersion = "0.2.0";
        private Logger _logger;
        private Game _game;
        private Config _config;
        private ushort _serverPort { get; }

        public Server(Logger logger, Game game, ushort port, string configJson)
        {
            _logger = logger;
            _game = game;
            _serverPort = port;
            _config = Config.LoadFromJson(game, configJson);
        }

        public void Start()
        {
            var build = $"League Sandbox Build {ServerContext.BuildDateString}";
            Console.Title = build;
            _logger.LogCoreInfo(build);
            _logger.LogCoreInfo($"Yorick {_serverVersion}");
            _logger.LogCoreInfo($"Game started on port: {_serverPort}");
            _game.Initialize(new Address(_serverHost, _serverPort), _blowfishKey, _config);
            _game.NetLoop();
        }

        public void Dispose()
        {
            // PathNode.DestroyTable();
        }
    }
}
