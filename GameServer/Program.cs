using System;
using Ninject;
using LeagueSandbox.GameServer;

namespace LeagueSandbox
{
    class Program
    {
        // TODO: Require consumers of this inject a ServerContext
        public static string ExecutingDirectory;
        private static StandardKernel Kernel;

        static void Main(string[] args)
        {
            Kernel = new StandardKernel();
            Kernel.Load(new Bindings());

            var context = Kernel.Get<ServerContext>();
            ExecutingDirectory = context.GetExecutingDirectory();

            var server = Kernel.Get<Server>();
            server.Start();

            Console.ReadLine();
        }

        [Obsolete("If you find yourself needing this method, do some refactoring so you don't need it. Prefer constructor injection. This will be removed in the future.")]
        public static T ResolveDependency<T>()
        {
            return Kernel.Get<T>();
        }
    }
}
