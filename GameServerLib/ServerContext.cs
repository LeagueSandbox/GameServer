using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace LeagueSandbox.GameServer
{
    public static class ServerContext
    {
        public static string ExecutingDirectory { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static Version Version { get; } = Assembly.GetEntryAssembly().GetName().Version;

        public static readonly DateTime BuildDate = new DateTime(2000, 1, 1).Add(new TimeSpan(
            TimeSpan.TicksPerDay * Version.Build +
            TimeSpan.TicksPerSecond * 2 * Version.Revision
        )).ToUniversalTime();

        public static string BuildDateString = BuildDate.ToString("G", CultureInfo.InvariantCulture) + " UTC";
    }
}
