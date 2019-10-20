using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using log4net;

namespace LeagueSandbox.GameServer.Logging
{
    public class LoggerProvider
    {
        static LoggerProvider()
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App.config");
            FileInfo finfo = new FileInfo(logFilePath);
            var logRepository = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            log4net.Config.XmlConfigurator.ConfigureAndWatch(logRepository, finfo);
        }

        public static ILog GetLogger()
        {
            var caller = new StackTrace().GetFrame(1).GetMethod().DeclaringType;
            return LogManager.GetLogger(caller);
        }
    }
}