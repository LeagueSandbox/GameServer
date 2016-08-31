using System;
using Ninject;
using LeagueSandbox.GameServer;

namespace LeagueSandbox
{
    class Program
    {
        // TODO: Require consumers of this inject a ServerContext
        public static string ExecutingDirectory;
        public static StandardKernel Kernel;

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
    }
}
