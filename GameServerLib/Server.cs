using System;
using ENet;
using LeagueSandbox.GameServer.Logic;

namespace LeagueSandbox.GameServer
{
    class Server : IDisposable
    {
        private string _blowfishKey = "17BLOhi6KZsTtldTsizvHg==";
        private uint _serverHost = Address.IPv4HostAny;
        private ushort _serverPort = Program.ServerPort;
        private string _serverVersion = "0.2.0";
        private Logger _logger;
        private ServerContext _serverContext;
        private Game _game;
        private Config _config;

        public Server(Logger logger, ServerContext serverContext, Game game)
        {
            _logger = logger;
            _serverContext = serverContext;
            _game = game;
            _config = Config.LoadFromJson(Program.ConfigJson);
        }

        public void Start()
        {
            _logger.LogCoreInfo($"Yorick {_serverVersion}");
            _logger.LogCoreInfo("Game started on port: {0}", _serverPort);
            _game.Initialize(new Address(_serverHost, _serverPort), _blowfishKey, _config);
            _game.NetLoop();
        }

        public void Dispose()
        {
            // PathNode.DestroyTable();
        }
    }
}
