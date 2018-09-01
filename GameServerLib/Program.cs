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
        public ushort ServerPort { get; private set; }

        public GameServerLauncher(ushort serverPort, string configJson, string blowfishKey) :
            this(serverPort, Config.LoadFromJson(configJson), blowfishKey, () => { })
        {

        }

        public GameServerLauncher(ushort serverPort, Config config, string blowfishKey, Action readyCallback)
        {
            ServerPort = serverPort;
            _logger = LoggerProvider.GetLogger();
            var itemManager = new ItemManager();
            game = new Game(itemManager);
            _server = new Server(game, serverPort, config, blowfishKey);

#if !DEBUG
            try
            {
#endif
                ExecutingDirectory = ServerContext.ExecutingDirectory;
                _server.Start(readyCallback);
#if !DEBUG
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
#endif
        }

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

        public void Stop()
        {
            _server.Stop();
        }
    }
}
