using System;
using System.IO;
using System.Runtime.ExceptionServices;

namespace LeagueSandbox.GameServer.Core.Logic
{
    public class Logger
    {
        private LogWriter _logWriter;

        public Logger(ServerContext serverContext)
        {
            _logWriter = new LogWriter(serverContext.GetExecutingDirectory(), "LeagueSandbox.txt");

            AppDomain.CurrentDomain.FirstChanceException += this.CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
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
            public string _executingDirectory;
            public string _logFileName;
            private object _locker = new object();

            public LogWriter(string executingDirectory, string logFileName)
            {
                _executingDirectory = executingDirectory;
                _logFileName = logFileName;
            }

            public void Log(string lines, string type = "LOG")
            {
                var text = string.Format("({0} {1}) [{2}]: {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), type.ToUpper(), lines);
                lock (_locker)
                {
                    File.AppendAllText(Path.Combine(_executingDirectory, "Logs", _logFileName), text + Environment.NewLine);
                    Console.WriteLine(text);
                }
            }

            public void CreateLogFile()
            {
                //Generate A Unique file to use as a log file
                if (!Directory.Exists(Path.Combine(_executingDirectory, "Logs")))
                    Directory.CreateDirectory(Path.Combine(_executingDirectory, "Logs"));

                _logFileName = string.Format("{0}T{1}{2}",
                    DateTime.Now.ToShortDateString().Replace("/", "_"),
                    DateTime.Now.ToShortTimeString().Replace(" ", "").Replace(":", "-"), "_" + _logFileName
                );

                var file = File.Create(Path.Combine(_executingDirectory, "Logs", _logFileName));

                file.Close();
            }
        }
    }

    
}
