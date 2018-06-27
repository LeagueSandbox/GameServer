using CommandLine;
using LeagueSandbox.GameServer;
using System.IO;

namespace LeagueSandbox.GameServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = ArgsOptions.Parse(args);

            string configJson = options.ConfigJson;
            if (string.IsNullOrEmpty(configJson))
            {
                configJson = File.ReadAllText(options.ConfigPath);
            }
            GameServerLauncher.LaunchServer(options.ServerPort, configJson);
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
