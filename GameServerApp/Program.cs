using System.IO;
using CommandLine;
using LeagueSandbox.GameServer;

namespace LeagueSandbox.GameServerApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var options = ArgsOptions.Parse(args);

            var configJson = options.ConfigJson;
            if (string.IsNullOrEmpty(configJson))
            {
                configJson = File.ReadAllText(options.ConfigPath);
            }

            var gameServerLauncher = new GameServerLauncher(options.ServerPort, configJson);
        }
    }

    public class ArgsOptions
    {
        [Option("config", DefaultValue = "Settings/GameInfo.json")]
        public string ConfigPath { get; set; }

        [Option("config-json", DefaultValue = "")]
        public string ConfigJson { get; set; }

        [Option("port", DefaultValue = (ushort)5119)]
        public ushort ServerPort { get; set; }

        public static ArgsOptions Parse(string[] args)
        {
            var options = new ArgsOptions();
            Parser.Default.ParseArguments(args, options);
            return options;
        }
    }
}
