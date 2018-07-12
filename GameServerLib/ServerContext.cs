using System.IO;
using System.Reflection;

namespace LeagueSandbox.GameServer
{
    public static class ServerContext
    {
        public static string ExecutingDirectory { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}
