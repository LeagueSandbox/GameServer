using System;
using System.IO;
using Ninject;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Logic.Content;

namespace LeagueSandbox
{
    class Program
    {
        // TODO: Require consumers of this inject a ServerContext
        public static string ExecutingDirectory { get; private set; }
        private static StandardKernel _kernel;
        public static bool IsSetToExit { get; set; }
        public static string GameInfoPath { get; private set; }

        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                Console.WriteLine("Lobby Server wants to use custom GameInfo file. Will use that if possible.");
                if (File.Exists(args[0]))
                {
                    GameInfoPath = args[0];
                }
            }

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
