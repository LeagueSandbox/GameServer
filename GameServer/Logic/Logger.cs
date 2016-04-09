using System;
using System.IO;
using System.Runtime.ExceptionServices;

namespace LeagueSandbox.GameServer.Core.Logic
{
    public static class Logger
    {
        public static void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            if (e.Exception is InvalidCastException || e.Exception is System.Collections.Generic.KeyNotFoundException)
                return;
            WriteToLog.Log("A first chance exception was thrown", "EXCEPTION");
            WriteToLog.Log(e.Exception.Message, "EXCEPTION");
            WriteToLog.Log(e.ToString(), "EXCEPTION");
        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs x)
        {
            WriteToLog.Log("An unhandled exception was thrown", "UNHANDLEDEXCEPTION");
            var ex = (Exception)x.ExceptionObject;
            WriteToLog.Log(ex.Message, "UNHANDLEDEXCEPTION");
            WriteToLog.Log(ex.ToString(), "UNHANDLEDEXCEPTION");
        }

        public static void Log(string line, string type = "LOG")
        {
            WriteToLog.Log(line, type);
        }

        public static void LogCoreInfo(string line)
        {
            Log(line, "CORE_INFO");
        }

        public static void LogCoreInfo(string format, params object[] args)
        {
            Log(string.Format(format, args), "CORE_INFO");
        }

        public static void LogCoreWarning(string line)
        {
            Log(line, "CORE_WARNING");
        }

        public static void LogCoreError(string line)
        {
            Log(line, "CORE_ERROR");
        }

        public static void LogCoreError(string format, params object[] args)
        {
            Log(string.Format(format, args), "CORE_ERROR");
        }
    }

    public static class WriteToLog
    {
        public static string ExecutingDirectory;
        public static string LogfileName;
        private static object locker = new object();

        public static void Log(string lines, string type = "LOG")
        {
            var text = string.Format("({0} {1}) [{2}]: {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), type.ToUpper(), lines);
            lock (locker)
            {
                File.AppendAllText(Path.Combine(ExecutingDirectory, "Logs", LogfileName), text + Environment.NewLine);
                Console.WriteLine(text);
            }
        }

        public static void CreateLogFile()
        {
            //Generate A Unique file to use as a log file
            if (!Directory.Exists(Path.Combine(ExecutingDirectory, "Logs")))
                Directory.CreateDirectory(Path.Combine(ExecutingDirectory, "Logs"));
            LogfileName = string.Format("{0}T{1}{2}", DateTime.Now.ToShortDateString().Replace("/", "_"),
                DateTime.Now.ToShortTimeString().Replace(" ", "").Replace(":", "-"), "_" + LogfileName);
            var file = File.Create(Path.Combine(ExecutingDirectory, "Logs", LogfileName));
            file.Close();
        }
    }
}
