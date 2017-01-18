using System;
using System.Runtime.InteropServices;

namespace LeagueSandbox.GameServer.Logic
{
	class PreciseTimer : IDisposable
	{
		int resolution;
		PreciseTimer(int r)
		{
			OperatingSystem os = Environment.OSVersion;
			PlatformID pid = os.Platform;
			switch (pid)
			{
				case PlatformID.Unix:
					break;
				default:
					{

						resolution = r;
						PreciseTimerWindows.timeBeginPeriod(resolution);
					}
					break;
			}
		}
		void IDisposable.Dispose()
		{
			OperatingSystem os = Environment.OSVersion;
			PlatformID pid = os.Platform;
			switch (pid)
			{
				case PlatformID.Unix:
					break;
				default:
					PreciseTimerWindows.timeEndPeriod(resolution);
					break;
			}
		}

		public static PreciseTimer SetResolution(int r)
		{
			return new PreciseTimer(r);
		}
	}

	class PreciseTimerWindows
	{
		[DllImport("winmm.dll")]
		public static extern int timeBeginPeriod(int msec);
		[DllImport("winmm.dll")]
		public static extern int timeEndPeriod(int msec);
	}
}
