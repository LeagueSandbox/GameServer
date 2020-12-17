using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Repository;

namespace LeagueSandbox.GameServer.Logging
{
    /// <summary>
    /// Class which creates logger instances.
    /// </summary>
    public static class LoggerProvider
    {
        /// <summary>
        /// Provider instance which configures log4net to prepare for getting a logger instance.
        /// </summary>
        static LoggerProvider()
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App.config");
            FileInfo finfo = new FileInfo(logFilePath);
            var rep = LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(rep, finfo);
        }

        /// <summary>
        /// Gets a logger instance specific to the caller.
        /// </summary>
        /// <returns>Logger designated to the specific caller.</returns>
        public static ILog GetLogger()
        {
            var caller = new StackTrace().GetFrame(1).GetMethod().DeclaringType;
            return LogManager.GetLogger(caller);
        }
    }
}