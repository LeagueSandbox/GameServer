using System;
using Ninject;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Logic.Content;
using CommandLine;

namespace LeagueSandbox
{
    class Program
    {
        // TODO: Require consumers of this inject a ServerContext
        public static string ExecutingDirectory { get; private set; }
        private static StandardKernel _kernel;
        public static bool IsSetToExit { get; set; }
        public static string ConfigPath { get; private set; }
        public static ushort ServerPort { get; private set; }

        static void Main(string[] args)
        {
            var options = new ArgsOptions();
            Parser.Default.ParseArguments(args, options);

            ConfigPath = options.ConfigPath;
            ServerPort = options.ServerPort;

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

    class ArgsOptions
    {
        [Option("path", DefaultValue = "Settings/GameInfo.json")]
        public string ConfigPath { get; set; }

        [Option("port", DefaultValue = (ushort)5119)]
        public ushort ServerPort { get; set; }
    }
}
