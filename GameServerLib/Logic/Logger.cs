using System;
using System.IO;
using System.Runtime.ExceptionServices;

namespace LeagueSandbox.GameServer.Core.Logic
{
    public class Logger
    {
        private LogWriter _logWriter;
        private const string _logName = "LeagueSandbox.txt";

        public Logger(ServerContext serverContext)
        {
            var directory = serverContext.ExecutingDirectory;
            _logWriter = new LogWriter(directory, _logName);

            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            if (e.Exception is InvalidCastException || e.Exception is System.Collections.Generic.KeyNotFoundException)
                return;
            _logWriter.Log("A first chance exception was thrown", "EXCEPTION");
            _logWriter.Log(e.Exception.Message, "EXCEPTION");
            _logWriter.Log(e.ToString(), "EXCEPTION");
        }

        public void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs x)
        {
            _logWriter.Log("An unhandled exception was thrown", "UNHANDLEDEXCEPTION");
            var ex = (Exception)x.ExceptionObject;
            _logWriter.Log(ex.Message, "UNHANDLEDEXCEPTION");
            _logWriter.Log(ex.ToString(), "UNHANDLEDEXCEPTION");
        }

        public void Log(string line, string type = "LOG")
        {
            _logWriter.Log(line, type);
        }

        public void LogCoreInfo(string line)
        {
            Log(line, "CORE_INFO");
        }

        public void LogCoreInfo(string format, params object[] args)
        {
            LogCoreInfo(string.Format(format, args));
        }

        public void LogCoreWarning(string line)
        {
            Log(line, "CORE_WARNING");
        }

        public void LogCoreWarning(string format, params object[] args)
        {
            LogCoreWarning(string.Format(format, args));
        }

        public void LogCoreError(string line)
        {
            Log(line, "CORE_ERROR");
        }

        public void LogCoreError(string format, params object[] args)
        {
            LogCoreError(string.Format(format, args));
        }

        private class LogWriter
        {
            public string _logFileName;
            private object _locker = new object();

            public LogWriter(string executingDirectory, string logFileName)
            {
                CreateLogFile(executingDirectory, logFileName);
            }

            public void Log(string lines, string type = "LOG")
            {
                var text = string.Format(
                    "({0} {1}) [{2}]: {3}",
                    DateTime.Now.ToShortDateString(),
                    DateTime.Now.ToShortTimeString(),
                    type.ToUpper(),
                    lines
                );
                lock (_locker)
                {
                    File.AppendAllText(
                        _logFileName,
                        text + Environment.NewLine
                    );
                    Console.WriteLine(text);
                }
            }

            public void CreateLogFile(string directory, string name)
            {
                if (!string.IsNullOrEmpty(_logFileName))
                {
                    return;
                }

                var path = Path.Combine(directory, "Logs");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var logName = string.Format(
                    "{0}-{1}",
                    DateTime.Now.ToString("yyyyMMdd-HHMM"),
                    name
                );
                _logFileName = Path.Combine(path, logName);

                var file = File.Create(_logFileName);

                file.Close();
            }
        }
    }
}
