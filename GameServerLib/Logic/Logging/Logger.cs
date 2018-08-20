using System;
using System.Diagnostics;
using log4net;

namespace LeagueSandbox.GameServer.Logic.Logging
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
                _log.Debug(AppendCallingMethodName(message?.ToString() ?? string.Empty));
        }

        public void Info(object message)
        {
            if (_log.IsInfoEnabled)
                _log.Info(AppendCallingMethodName(message?.ToString() ?? string.Empty));
        }

        public void Warning(object message)
        {
            if (_log.IsWarnEnabled)
                _log.Warn(AppendCallingMethodName(message?.ToString() ?? string.Empty));
        }

        public void Error(object message)
        {
            if (_log.IsErrorEnabled)
                _log.Error(AppendCallingMethodName(message?.ToString() ?? string.Empty));
        }

        public void Error(Exception ex)
        {
            if (_log.IsErrorEnabled)
                _log.Error(AppendCallingMethodName(ex?.Message ?? string.Empty), ex);
        }

        public void Error(object message, Exception ex)
        {
            if (_log.IsErrorEnabled)
                _log.Error(AppendCallingMethodName(message?.ToString() ?? string.Empty), ex);
        }

        private string AppendCallingMethodName(string message)
        {
            var stackTrace = new StackTrace();
            if (stackTrace.FrameCount < 3)
                return message;

            try
            {
                var methodBase = stackTrace.GetFrame(2).GetMethod();
                return $"{methodBase?.ReflectedType?.FullName}.{methodBase?.Name} : {message}";
            }
            catch (Exception ex)
            {
                _log.Error($"{GetType().FullName}.{nameof(AppendCallingMethodName)}] - Log error", ex);
            }

            return message;
        }
    }
}
