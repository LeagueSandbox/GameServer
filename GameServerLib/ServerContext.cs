using System.IO;
using System.Reflection;

namespace LeagueSandbox.GameServer
{
    public class ServerContext
    {
        public string ExecutingDirectory { get; private set; }

        public ServerContext()
        {
            ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
