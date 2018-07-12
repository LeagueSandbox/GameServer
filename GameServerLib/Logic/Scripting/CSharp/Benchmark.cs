using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Scripting.CSharp
{
    public static class Benchmark
    {
        private static IDictionary<string, Stopwatch> _map = new Dictionary<string, Stopwatch>();
        public static void StartTiming(string label)
        {
            var stopwatch = new Stopwatch();
            _map[label] = stopwatch;
            stopwatch.Reset();
            stopwatch.Start();
        }

        public static void EndTiming(string label)
        {
            var stopwatch = _map[label];
            stopwatch.Stop();
            var t = Task.Factory.StartNew(() =>
            {
                Logger.LogCoreInfo($"{label} Elapsed(MS) = {stopwatch.Elapsed.TotalMilliseconds} - FPS: {1000/stopwatch.Elapsed.TotalMilliseconds}");
            });
            _map.Remove(label);
        }

        public static void Log(string text)
        {
            var t = Task.Factory.StartNew(() => Logger.LogCoreInfo(text));
        }
    }
}
