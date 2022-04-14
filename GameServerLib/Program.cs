using LeagueSandbox.GameServer.Logging;
using log4net;
using LeagueSandbox.GameServer.Inventory;
using System;

namespace LeagueSandbox.GameServer
{
    /// <summary>
    /// This class is used by the Program class to launch the server.
    /// Ideally the Program class in this project would be removed entirely, but that's not possible yet.
    /// </summary>
    public class GameServerLauncher
    {
        // Crucial Vars
        private readonly ILog _logger;
        private readonly Server _server;
        public Game game;

        /// <summary>
        /// Directory that the GameServerConsole was ran from.
        /// </summary>
        public string ExecutingDirectory { get; private set; }
        /// <summary>
        /// String representing the content of the game configuration file.
        /// </summary>
        public string ConfigJson { get; private set; }
        /// <summary>
        /// Port that the GameServer will be hosted on.
        /// </summary>
        public ushort ServerPort { get; private set; }

        /// <summary>
        /// Represents the class which will call for the GameServer to start.
        /// </summary>
        /// <param name="serverPort">Launch argument representing the port the server will be hosted on.</param>
        /// <param name="configJson">Launch argument representing the configuration file that houses all information about the game such as players, champions, map, etc.</param>
        /// <param name="blowfishKey">Singular blowfish key which the GameServer will run on. *NOTE*: Will be depricated once per-peer blowfish keys are implemented.</param>
        public GameServerLauncher(ushort serverPort, string configJson)
        {
            ConfigJson = configJson;
            ServerPort = serverPort;
            _logger = LoggerProvider.GetLogger();
            game = new Game();

            _server = new Server(game, serverPort, configJson);

#if !DEBUG
            try
            {
#endif
                // Where the server first initializes.
                ExecutingDirectory = ServerContext.ExecutingDirectory;
                _server.Start();
#if !DEBUG
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
#endif
        }

        /// <summary>
        /// Function which attempts to start the network loop for the GameServer.
        /// </summary>
        public void StartNetworkLoop()
        {
#if !DEBUG
            try
            {
#endif
                _server.StartNetworkLoop();
#if !DEBUG
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
#endif
        }
    }
}
