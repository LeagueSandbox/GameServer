using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer
{
    class Server : IDisposable
    {
        private uint SERVER_HOST = Address.IPv4HostAny;
        private ushort SERVER_PORT = 5119;
        private string SERVER_KEY = "17BLOhi6KZsTtldTsizvHg==";
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
            Console.WriteLine("Yorick " + SERVER_VERSION);

            _logger.LogCoreInfo("Game started");
            
            var address = new Address(SERVER_HOST, SERVER_PORT);

            if (!_game.Initialize(address, SERVER_KEY))
            {
                _logger.LogCoreError("Couldn't listen on port " + SERVER_PORT + ", or invalid key");
                throw new ApplicationException();
            }

            _game.NetLoop();
        }

        public void Dispose()
        {
            PathNode.DestroyTable();
        }
    }
}
