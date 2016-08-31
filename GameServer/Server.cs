using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.GameObjects;

namespace LeagueSandbox.GameServer
{
    class Server : IDisposable
    {
        private uint SERVER_HOST = Address.IPv4HostAny;
        private ushort SERVER_PORT = 5119;
        private string SERVER_KEY = "17BLOhi6KZsTtldTsizvHg==";
        private string SERVER_VERSION = "0.2.0";
        private Logger _logger;
        private RAFManager _rafManager;
        private ServerContext _serverContext;

        public Server(Logger logger, RAFManager rafManager, ServerContext serverContext)
        {
            _logger = logger;
            _rafManager = rafManager;
            _serverContext = serverContext;
        }

        public void Start()
        {
            Console.WriteLine("Yorick " + SERVER_VERSION);

            _logger.LogCoreInfo("Game started");

            var game = new Game();
            var address = new Address(SERVER_HOST, SERVER_PORT);

            if (!game.Initialize(address, SERVER_KEY))
            {
                _logger.LogCoreError("Couldn't listen on port " + SERVER_PORT + ", or invalid key");
                throw new ApplicationException();
            }

            game.NetLoop();
        }

        public void Dispose()
        {
            PathNode.DestroyTable();
        }
    }
}
