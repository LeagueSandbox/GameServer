using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LeagueSandbox.GameServer
{
    /// <summary>
    /// Class which houses the build information of the currently running build of the Server.
    /// </summary>
    public static class ServerContext
    {
        public static string ExecutingDirectory => Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location
        );

        public static string BuildDateString => (
            Attribute.GetCustomAttribute(
                Assembly.GetExecutingAssembly(),
                typeof(BuildDateTimeAttribute)
            ) as BuildDateTimeAttribute
        ).Date;
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class BuildDateTimeAttribute : Attribute
    {
        public string Date { get; private set; }
        public BuildDateTimeAttribute(string date)
        {
            Date = date;
        }
    }
}
