using System;
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

        static void Main(string[] args)
        {
            _kernel = new StandardKernel();
            _kernel.Load(new Bindings());

            var context = _kernel.Get<ServerContext>();
            var server = _kernel.Get<Server>();
            var itemManager = _kernel.Get<ItemManager>();

            ExecutingDirectory = context.ExecutingDirectory;
            itemManager.LoadItems();
            server.Start();

            Console.ReadLine();
        }

        [Obsolete("If you find yourself needing this method, do some refactoring so you don't need it. Prefer constructor injection. This will be removed in the future.")]
        public static T ResolveDependency<T>()
        {
            return _kernel.Get<T>();
        }
    }
}
