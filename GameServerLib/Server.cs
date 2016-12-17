using System;
using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic;

namespace LeagueSandbox.GameServer
{
    class Server : IDisposable
    {
        private string BLOWFISH_KEY = "17BLOhi6KZsTtldTsizvHg==";
        private uint SERVER_HOST = Address.IPv4HostAny;
        private ushort SERVER_PORT = Program.ServerPort;
        private string SERVER_VERSION = "0.2.0";
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
            _logger.LogCoreInfo($"Yorick {SERVER_VERSION}");
            _logger.LogCoreInfo("Game started on port: {0}", SERVER_PORT);
            _game.Initialize(new Address(SERVER_HOST, SERVER_PORT), BLOWFISH_KEY, _config);
            _game.NetLoop();
        }

        public void Dispose()
        {
            PathNode.DestroyTable();
        }
    }
}
