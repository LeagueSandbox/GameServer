using CommandLine;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Logic;

namespace LeagueSandbox.GameServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = ArgsOptions.Parse(args);
            Config config;
            if (string.IsNullOrEmpty(options.ConfigJson))
            {
                config = Config.LoadFromFile(options.ConfigPath);
            }
            else
            {
                config = Config.LoadFromJson(options.ConfigJson);
            }
            GameServerLauncher.LaunchServer(options.ServerPort, config);
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
