using System;
using System.Diagnostics;
using log4net;

namespace LeagueSandbox.GameServer.Logging
{
    public class Logger : ILogger
    {
        private readonly ILog _log;

        public Logger(ILog log)
        {
            _log = log;
        }

        public void Debug(object message)
        {
            if (_log.IsDebugEnabled)
                _log.Debug(message?.ToString() ?? string.Empty);
        }

        public void Info(object message)
        {
            if (_log.IsInfoEnabled)
                _log.Info(message?.ToString() ?? string.Empty);
        }

        public void Warning(object message)
        {
            if (_log.IsWarnEnabled)
                _log.Warn(message?.ToString() ?? string.Empty);
        }

        public void Error(object message)
        {
            if (_log.IsErrorEnabled)
                _log.Error(message?.ToString() ?? string.Empty);
        }

        public void Error(Exception ex)
        {
            if (_log.IsErrorEnabled)
                _log.Error(ex?.Message ?? string.Empty, ex);
        }

        public void Error(object message, Exception ex)
        {
            if (_log.IsErrorEnabled)
                _log.Error(message?.ToString() ?? string.Empty, ex);
        }
    }
}
