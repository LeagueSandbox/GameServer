using System.Reflection;

namespace LeagueSandbox.GameServer
{
    public class ServerContext
    {
        public string ExecutingDirectory { get; private set; }

        public ServerContext()
        {
            ExecutingDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
