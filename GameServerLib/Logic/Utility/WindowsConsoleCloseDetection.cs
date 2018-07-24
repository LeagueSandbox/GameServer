using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Utility
{
    public class WindowsConsoleCloseDetection
    {
        static List<ConsoleEventDelegate> handlerList = new List<ConsoleEventDelegate>();
        public delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        public static void SetCloseHandler(ConsoleEventDelegate handler)
        {
            handlerList.Add(handler);
            SetConsoleCtrlHandler(handler, true);
        }
    }
}
