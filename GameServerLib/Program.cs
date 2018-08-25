using System;
using System.Timers;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.Logging;

namespace LeagueSandbox.GameServer
{
    /// <summary>
    /// This class is used by the GameServerApp to launch the server.
    /// Ideally the Program class in this project would be removed entirely, but that's not possible yet.
    /// </summary>
    public class GameServerLauncher
    {
        private readonly ILogger _logger;
        private readonly Server _server;

        public Game game;
        public string ExecutingDirectory { get; private set; }
        public string ConfigJson { get; private set; }
        public ushort ServerPort { get; private set; }

        public GameServerLauncher(ushort serverPort, string configJson, string blowfishKey)
        {
            ConfigJson = configJson;
            ServerPort = serverPort;
            _logger = LoggerProvider.GetLogger();
            var itemManager = new ItemManager();
            game = new Game(itemManager);
            _server = new Server(game, serverPort, configJson, blowfishKey);

            try
            {
                ExecutingDirectory = ServerContext.ExecutingDirectory;
                itemManager.LoadItems();
                _server.Start();
            }
            catch (Exception e)
            {
                _logger.Error(e);
#if DEBUG
                throw;
#endif
            }
        }

        public void StartNetworkLoop()
        {
            try
            {
                _server.StartNetworkLoop();
            }
            catch (Exception e)
            {
                _logger.Error(e);
#if DEBUG
                throw;
#endif
            }
        }
    }
}
