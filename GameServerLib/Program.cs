using System;
using Ninject;
using LeagueSandbox.GameServer.Logic.Content;

namespace LeagueSandbox.GameServer
{
    /// <summary>
    /// This class is used by the GameServerApp to launch the server.
    /// Ideally the Program class in this project would be removed entirely, but that's not possible yet.
    /// </summary>
    public class GameServerLauncher
    {
        public static void LaunchServer(string configPath, ushort serverPort)
        {
            Program.Run(configPath, serverPort);
        }
    }

    class Program
    {
        // TODO: Require consumers of this inject a ServerContext
        public static string ExecutingDirectory { get; private set; }
        private static StandardKernel _kernel;
        public static bool IsSetToExit { get; set; }
        public static string ConfigPath { get; private set; }
        public static ushort ServerPort { get; private set; }

        public static void Run(string configPath, ushort serverPort)
        {
            ConfigPath = configPath;
            ServerPort = serverPort;

            _kernel = new StandardKernel();
            _kernel.Load(new Bindings());

            var context = _kernel.Get<ServerContext>();
            var server = _kernel.Get<Server>();
            var itemManager = _kernel.Get<ItemManager>();

            ExecutingDirectory = context.ExecutingDirectory;
            itemManager.LoadItems();
            server.Start();
        }

        public static void SetToExit()
        {
            Console.WriteLine("Game is over. Game Server will exit in 10 seconds.");
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
