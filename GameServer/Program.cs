using System;
using Ninject;
using LeagueSandbox.GameServer;

namespace LeagueSandbox
{
    class Program
    {
        // TODO: Require consumers of this inject a ServerContext
        public static string ExecutingDirectory;

        static void Main(string[] args)
        {
            var kernel = new StandardKernel();
            kernel.Load(new Bindings());

            var context = kernel.Get<ServerContext>();
            ExecutingDirectory = context.GetExecutingDirectory();

            var server = kernel.Get<Server>();
            server.Start();

            Console.ReadLine();
        }
    }
}
