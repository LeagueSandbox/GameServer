using System;
using System.Timers;
using LeagueSandbox.GameServer.Logic;
using LeagueSandbox.GameServer.Logic.Content;

namespace LeagueSandbox.GameServer
{
    /// <summary>
    /// This class is used by the GameServerApp to launch the server.
    /// Ideally the Program class in this project would be removed entirely, but that's not possible yet.
    /// </summary>
    public class GameServerLauncher
    {
        public Logger logger;
        public Game game;
        public string ExecutingDirectory { get; private set; }
        public string ConfigJson { get; private set; }
        public ushort ServerPort { get; private set; }
        private Server server;

        public GameServerLauncher(ushort serverPort, string configJson, string blowfishKey, Logger logger)
        {
            ConfigJson = configJson;
            ServerPort = serverPort;
            this.logger = logger;
            var itemManager = new ItemManager();
            game = new Game(itemManager, logger);
            server = new Server(logger, game, serverPort, configJson, blowfishKey);

            try
            {
                ExecutingDirectory = ServerContext.ExecutingDirectory;
                itemManager.LoadItems();
                server.Start();
            }
            catch (Exception e)
            {
                logger.LogFatalError("Error: {0}", e.ToString());
                #if DEBUG
                throw;
                #endif
            }
        }
        public void StartNetworkLoop()
        {
            try
            {
                server.StartNetworkLoop();
            }
            catch (Exception e)
            {
                logger.LogFatalError("Error: {0}", e.ToString());
                #if DEBUG
                throw;
                #endif
            }
        }
    }
}
