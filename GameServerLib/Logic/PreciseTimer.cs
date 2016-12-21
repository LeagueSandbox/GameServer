using System;
using System.Runtime.InteropServices;

namespace LeagueSandbox.GameServer.Logic
{
    class PreciseTimer : IDisposable
    {
        int resolution;
        PreciseTimer(int r)
        {
            resolution = r;
            timeBeginPeriod(resolution);
        }
        void IDisposable.Dispose()
        {
            timeEndPeriod(resolution);
        }

        public static PreciseTimer SetResolution(int r)
        {
            return new PreciseTimer(r);
        }
        [DllImport("winmm.dll")]
        private static extern int timeBeginPeriod(int msec);
        [DllImport("winmm.dll")]
        private static extern int timeEndPeriod(int msec);
    }
}
