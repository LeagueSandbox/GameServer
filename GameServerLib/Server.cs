using System;
using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;

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

        public Server(Logger logger, ServerContext serverContext, Game game)
        {
            _logger = logger;
            _serverContext = serverContext;
            _game = game;
        }

        public void Start()
        {
            Console.WriteLine($"Yorick {SERVER_VERSION}");
            _logger.LogCoreInfo("Game started");
            _game.Initialize(new Address(SERVER_HOST, SERVER_PORT), BLOWFISH_KEY);
            _game.NetLoop();
        }

        public void Dispose()
        {
            PathNode.DestroyTable();
        }
    }
}
