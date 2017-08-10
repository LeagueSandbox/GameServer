using System;
using Ninject;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.DependencyInjection;

namespace LeagueSandbox.GameServer
{
    /// <summary>
    /// This class is used by the GameServerApp to launch the server.
    /// Ideally the Program class in this project would be removed entirely, but that's not possible yet.
    /// </summary>
    public class GameServerLauncher
    {
        public static void LaunchServer(ushort serverPort, string configJson)
        {
            Program.Run(serverPort, configJson);
        }
    }

    class Program
    {
        // TODO: Require consumers of this inject a ServerContext
        public static string ExecutingDirectory { get; private set; }
        private static StandardKernel _kernel;
        public static bool IsSetToExit { get; set; }
        public static string ConfigJson { get; private set; }
        public static ushort ServerPort { get; private set; }

        public static void Run(ushort serverPort, String configJson)
        {
            ConfigJson = configJson;
            ServerPort = serverPort;

            _kernel = new StandardKernel();
            _kernel.Load(new Bindings());
            DI.Container = _kernel;

            var context = _kernel.Get<ServerContext>();
            var server = _kernel.Get<Server>();
            var itemManager = _kernel.Get<ItemManager>();
            var logger = _kernel.Get<Logger>();

            try
            {
                ExecutingDirectory = context.ExecutingDirectory;
                itemManager.LoadItems();
                server.Start();
            }
            catch (Exception e)
            {
                logger.LogFatalError("Error: {0}", e.ToString());
            }
        }

        public static void SetToExit()
        {
            Logger _logger = Program.ResolveDependency<Logger>();
            _logger.LogCoreInfo("Game is over. Game Server will exit in 10 seconds.");
            var timer = new System.Timers.Timer(10000) { AutoReset = false };
            timer.Elapsed += (a, b) => IsSetToExit = true;
            timer.Start();
        }

        [Obsolete("If you find yourself needing this method, do some refactoring so you don't need it. Prefer constructor injection. This will be removed in the future.")]
        public static T ResolveDependency<T>()
        {
            return _kernel.Get<T>();
        }
    }
}
