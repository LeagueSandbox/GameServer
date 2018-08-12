using System;
using System.Diagnostics;
using log4net;

namespace LeagueSandbox.GameServer.Logic.Logging
{
    public class LoggerProvider : ILoggerProvider
    {
        static LoggerProvider()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static ILogger GetLogger()
        {
            var caller = new StackTrace().GetFrame(1).GetMethod().DeclaringType;
            return new Logger(LogManager.GetLogger(caller));
        }

        ILogger ILoggerProvider.GetLogger()
        {
            return GetLogger();
        }
    }
}
