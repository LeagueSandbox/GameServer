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
        }
        void IDisposable.Dispose()
        {
        }

        public static PreciseTimer SetResolution(int r)
        {
            return new PreciseTimer(r);
        }
    }
}
