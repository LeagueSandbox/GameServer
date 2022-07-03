using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using GameServerConsole.Properties;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServerConsole.Logic;
using LeagueSandbox.GameServerConsole.Utility;
using log4net;

namespace LeagueSandbox.GameServerConsole
{
    /// <summary>
    /// Class representing the program piece, or commandline piece of the server; where everything starts (GameServerConsole -> GameServer, etc).
    /// </summary>
    internal class Program
    {
        // So we can print debug info via the command line interface.
        private static ILog _logger = LoggerProvider.GetLogger();

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(
                (sender, args) => _logger.Fatal(null, (Exception)args.ExceptionObject)
            );

            // If the command line interface was ran with additional parameters (perhaps via a shortcut or just via another command line)
            // Refer to ArgsOptions for all possible launch parameters
            var parsedArgs = ArgsOptions.Parse(args);
            parsedArgs.GameInfoJson = LoadConfig(
                parsedArgs.GameInfoJsonPath,
                parsedArgs.GameInfoJson,
                Encoding.UTF8.GetString(Resources.GameInfo));

            var gameServerLauncher = new GameServerLauncher(
                parsedArgs.ServerPort,
                parsedArgs.GameInfoJson);

#if DEBUG
            // When debugging, optionally the game client can be launched automatically given the path (placed in GameServerSettings.json) to the folder containing the League executable.
            var configGameServerSettings = GameServerConfig.LoadFromJson(LoadConfig(
                parsedArgs.GameServerSettingsJsonPath,
                parsedArgs.GameServerSettingsJson,
                Encoding.UTF8.GetString(Resources.GameServerSettings)));

            if (configGameServerSettings.AutoStartClient)
            {
                var leaguePath = configGameServerSettings.ClientLocation;
                if (Directory.Exists(leaguePath))
                {
                    leaguePath = Path.Combine(leaguePath, "League of Legends.exe");
                }
                if (File.Exists(leaguePath))
                {
                    // TODO: launch a client for each player in config
                    var startInfo = new ProcessStartInfo(leaguePath)
                    {
                        Arguments = String.Format("\"8394\" \"LoLLauncher.exe\" \"\" \"127.0.0.1 {0} {1} 1\"",
                            parsedArgs.ServerPort, gameServerLauncher.game.Config.Players.First().BlowfishKey),
                        WorkingDirectory = Path.GetDirectoryName(leaguePath)
                    };

                    var leagueProcess = Process.Start(startInfo);

                    _logger.Info("Launching League of Legends. You can disable this in GameServerSettings.json.");

                    if (Environment.OSVersion.Platform == PlatformID.Win32NT ||
                        Environment.OSVersion.Platform == PlatformID.Win32S ||
                        Environment.OSVersion.Platform == PlatformID.Win32Windows ||
                        Environment.OSVersion.Platform == PlatformID.WinCE)
                    {
                        WindowsConsoleCloseDetection.SetCloseHandler((_) =>
                        {
                            if (!leagueProcess.HasExited)
                            {
                                leagueProcess.Kill();
                            }
                            return true;
                        });
                    }
                }
                else
                {
                    _logger.Warn("Unable to find League of Legends.exe. Check the GameServerSettings.json settings and your League location.");
                }
            }
            else
            {
                _logger.Info("Server is ready, clients can now connect.");
            }
#endif
            // This is where the actual GameServer starts.
            gameServerLauncher.StartNetworkLoop();
        }

        /// <summary>
        /// Used to parse any of the configuration files used for the GameServer, ex: GameInfo.json or GameServerSettings.json. 
        /// </summary>
        /// <param name="filePath">Full path to the configuration file.</param>
        /// <param name="currentJsonString">String representing the content of the configuration file. Usually empty.</param>
        /// <param name="defaultJsonString">String representing the default content of the configuration file. Usually what is already defined in the respective configuration file.</param>
        /// <returns>The string defined in the configuration file or defined via launch arguments.</returns>
        private static string LoadConfig(string filePath, string currentJsonString, string defaultJsonString)
        {
            if (!string.IsNullOrEmpty(currentJsonString))
                return currentJsonString;

            try
            {
                if (File.Exists(filePath))
                    return File.ReadAllText(filePath);

                var settingsDirectory = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(settingsDirectory))
                    throw new Exception(string.Format("Creating Config File failed. Invalid Path: {0}", filePath));

                Directory.CreateDirectory(settingsDirectory);

                File.WriteAllText(filePath, defaultJsonString);
            }
            catch (Exception e)
            {
                _logger.Error(null, e);
            }

            return defaultJsonString;
        }
    }

    /// <summary>
    /// Class housing launch arguments and their parsing used for the GameServerConsole.
    /// </summary>
    public class ArgsOptions
    {
        [Option("config", Default = "Settings/GameInfo.json")]
        public string GameInfoJsonPath { get; set; }

        [Option("config-gameserver", Default = "Settings/GameServerSettings.json")]
        public string GameServerSettingsJsonPath { get; set; }

        [Option("config-json", Default = "")]
        public string GameInfoJson { get; set; }

        [Option("config-gameserver-json", Default = "")]
        public string GameServerSettingsJson { get; set; }

        [Option("port", Default = (ushort)5119)]
        public ushort ServerPort { get; set; }

        public static ArgsOptions Parse(string[] args)
        {
            ArgsOptions options = null;
            Parser.Default.ParseArguments<ArgsOptions>(args).WithParsed(argOptions => options = argOptions);
            return options;
        }
    }
}
