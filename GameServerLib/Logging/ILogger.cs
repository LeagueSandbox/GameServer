using System;

namespace LeagueSandbox.GameServer.Logging
{
    public interface ILogger
    {
        void Debug(object message);
        void Info(object message);
        void Warning(object message);
        void Error(object message);
        void Error(Exception ex);
        void Error(object message, Exception ex);
    }
}
