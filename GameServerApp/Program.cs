using CommandLine;
using LeagueSandbox.GameServer;

namespace LeagueSandbox.GameServerApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = ArgsOptions.Parse(args);
            GameServerLauncher.LaunchServer(options.ConfigPath, options.ServerPort);
        }
    }

    public class ArgsOptions
    {
        [Option("config", DefaultValue = "Settings/GameInfo.json")]
        public string ConfigPath { get; set; }

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
