using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public class Benchmark
    {
        private IDictionary<string, Stopwatch> _map = new Dictionary<string, Stopwatch>();
        private static ILog _logger = LoggerProvider.GetLogger();

        public void StartTiming(string label)
        {
            var stopwatch = new Stopwatch();
            _map[label] = stopwatch;
            stopwatch.Reset();
            stopwatch.Start();
        }

        public void EndTiming(string label)
        {
            var stopwatch = _map[label];
            stopwatch.Stop();
            var t = Task.Factory.StartNew(() =>
            {
                _logger.Debug($"{label} Elapsed(MS) = {stopwatch.Elapsed.TotalMilliseconds} - FPS: {1000 / stopwatch.Elapsed.TotalMilliseconds}");
            });
            _map.Remove(label);
        }

        public void Log(string text)
        {
            var t = Task.Factory.StartNew(() => _logger.Debug(text));
        }
    }
}
