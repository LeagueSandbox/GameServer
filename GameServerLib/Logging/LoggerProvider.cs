﻿using System;
using System.Diagnostics;
using log4net;

namespace LeagueSandbox.GameServer.Logging
{
    public class LoggerProvider
    {
        static LoggerProvider()
        {
            log4net.Config.XmlConfigurator.Configure(LogManager.CreateRepository(Guid.NewGuid().ToString()));
        }

        public static ILog GetLogger()
        {
            var caller = new StackTrace().GetFrame(1).GetMethod().DeclaringType;
            return LogManager.GetLogger(caller);
        }
    }
}