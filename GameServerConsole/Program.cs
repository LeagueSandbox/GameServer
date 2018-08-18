using System;
using System.Diagnostics;
using System.IO;
using CommandLine;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Logic.Logging;
using LeagueSandbox.GameServerApp.Logic;
using LeagueSandbox.GameServerApp.Utility;

namespace GameServerConsole
{
    class Program
    {
        private static ILogger _logger;

        private static void Main(string[] args)
        {
            ArgsOptions.Parse(args).WithParsed(options =>
            {
                var configJson = options.ConfigJson;
                if (string.IsNullOrEmpty(configJson))
                {
                    configJson = File.ReadAllText(options.ConfigPath);
                }

                var configGameServerSettingsJson = options.ConfigGameServerSettingsJson;
                GameServerConfig configGameServerSettings;
                if (string.IsNullOrEmpty(configGameServerSettingsJson))
                {
                    configGameServerSettings = GameServerConfig.LoadFromFile(options.ConfigGameServerPath);
                }
                else
                {
                    configGameServerSettings = GameServerConfig.Default();
                }

                string blowfishKey = "17BLOhi6KZsTtldTsizvHg==";

                _logger = LoggerProvider.GetLogger();
                var gameServerLauncher = new GameServerLauncher(options.ServerPort, configJson, blowfishKey);
#if DEBUG
                if (configGameServerSettings.AutoStartClient)
                {
                    string leaguePath = configGameServerSettings.ClientLocation;
                    if (Directory.Exists(leaguePath))
                    {
                        leaguePath = Path.Combine(leaguePath, "League of Legends.exe");
                    }
                    if (File.Exists(leaguePath))
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo(leaguePath);
                        startInfo.Arguments = String.Format("\"8394\" \"LoLLauncher.exe\" \"\" \"127.0.0.1 {0} {1} 1\"", options.ServerPort, blowfishKey);
                        startInfo.WorkingDirectory = Path.GetDirectoryName(leaguePath);
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
                        _logger.Info("Unable to find League of Legends.exe. Check the GameServerSettings.json settings and your League location.");
                    }
                }
#endif
                gameServerLauncher.StartNetworkLoop();
            });
        }
    }

    public class ArgsOptions
    {
        [Option("config", Default = "Settings/GameInfo.json")]
        public string ConfigPath { get; set; }

        [Option("config-gameserver", Default = "Settings/GameServerSettings.json")]
        public string ConfigGameServerPath { get; set; }

        [Option("config-json", Default = "")]
        public string ConfigJson { get; set; }

        [Option("config-gameserver-json", Default = "")]
        public string ConfigGameServerSettingsJson { get; set; }

        [Option("port", Default = (ushort)5119)]
        public ushort ServerPort { get; set; }

        public static ParserResult<ArgsOptions> Parse(string[] args)
        {
            return Parser.Default.ParseArguments<ArgsOptions>(args);
        }
    }
}