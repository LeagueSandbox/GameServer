using System;
using ENet;
using LeagueSandbox.GameServer.Logic;

namespace LeagueSandbox.GameServer
{
    internal class Server : IDisposable
    {
        private string _blowfishKey;
        private uint _serverHost = Address.IPv4HostAny;
        private string _serverVersion = "0.2.0";
        private Logger _logger;
        private Game _game;
        private Config _config;
        private ushort _serverPort { get; }

        public Server(Logger logger, Game game, ushort port, string configJson, string blowfishKey)
        {
            _logger = logger;
            _game = game;
            _serverPort = port;
            _blowfishKey = blowfishKey;
            _config = Config.LoadFromJson(game, configJson);
        }

        public void Start()
        {
            _logger.LogCoreInfo($"Yorick {_serverVersion}");
            _logger.LogCoreInfo("Game started on port: {0}", _serverPort);
            _game.Initialize(new Address(_serverHost, _serverPort), _blowfishKey, _config);
        }

        public void StartNetworkLoop()
        {
            _game.NetLoop();
        }

        public void Dispose()
        {
            // PathNode.DestroyTable();
        }
    }
}
