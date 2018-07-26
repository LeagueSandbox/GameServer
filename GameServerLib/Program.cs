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

        public GameServerLauncher(ushort serverPort, string configJson, string blowfishKey, Action<Logger> onServerStarted)
        {
            ConfigJson = configJson;
            ServerPort = serverPort;
            logger = new Logger();
            var itemManager = new ItemManager();
            game = new Game(itemManager, logger);
            var server = new Server(logger, game, serverPort, configJson, blowfishKey);

            try
            {
                ExecutingDirectory = ServerContext.ExecutingDirectory;
                itemManager.LoadItems();
                server.Start(onServerStarted);
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
