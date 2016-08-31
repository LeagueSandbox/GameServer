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
        private RAFManager _rafManager;
        private ServerContext _serverContext;

        public Server(RAFManager rafManager, ServerContext serverContext)
        {
            _rafManager = rafManager;
            _serverContext = serverContext;
        }

        public void Start()
        {
            Console.WriteLine("Yorick " + SERVER_VERSION);

            InitalizeLogger();
            LoadRAFFiles();

            Logger.LogCoreInfo("Game started");

            var game = new Game();
            var address = new Address(SERVER_HOST, SERVER_PORT);

            if (!game.Initialize(address, SERVER_KEY))
            {
                Logger.LogCoreError("Couldn't listen on port " + SERVER_PORT + ", or invalid key");
                throw new ApplicationException();
            }

            game.NetLoop();
        }

        public void Dispose()
        {
            PathNode.DestroyTable();
        }

        private void InitalizeLogger()
        {
            WriteToLog.ExecutingDirectory = _serverContext.GetExecutingDirectory();
            WriteToLog.LogfileName = "LeagueSandbox.txt";
            WriteToLog.CreateLogFile();

            AppDomain.CurrentDomain.FirstChanceException += Logger.CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += Logger.CurrentDomain_UnhandledException;
        }

        private void LoadRAFFiles()
        {
            Logger.LogCoreInfo("Loading RAF files in filearchives/.");

            var settings = Settings.Load("Settings/Settings.json");
            if (!RAFManager.getInstance().init(System.IO.Path.Combine(settings.RadsPath, "filearchives")))
            {
                Logger.LogCoreError("Couldn't load RAF files. Make sure you have a 'filearchives' directory in the server's root directory. This directory is to be taken from RADS/projects/lol_game_client/");
            }
        }
    }
}
