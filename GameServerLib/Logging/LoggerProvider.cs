using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Repository;

namespace LeagueSandbox.GameServer.Logging
{
    public static class LoggerProvider
    {
        static LoggerProvider()
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App.config");
            FileInfo finfo = new FileInfo(logFilePath);
            var rep = LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(rep, finfo);
        }

        public static ILog GetLogger()
        {
            var caller = new StackTrace().GetFrame(1).GetMethod().DeclaringType;
            return LogManager.GetLogger(caller);
        }
    }
}