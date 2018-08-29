using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using CommandLine;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServerApp.Logic;
using LeagueSandbox.GameServerApp.Properties;
using LeagueSandbox.GameServerApp.Utility;

namespace LeagueSandbox.GameServerApp
{
    internal class Program
    {
        private static ILogger _logger;

        private static void Main(string[] args)
        {
            _logger = LoggerProvider.GetLogger();

            var parsedArgs = ArgsOptions.Parse(args);
            var settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "Settings");

            if (string.IsNullOrEmpty(parsedArgs.GameInfoJson))
            {
                if (File.Exists(parsedArgs.GameInfoJsonPath))
                {
                    parsedArgs.GameInfoJson = File.ReadAllText(parsedArgs.GameInfoJsonPath);
                }
                else
                {
                    try
                    {
                        Directory.CreateDirectory(settingsPath);

                        parsedArgs.GameInfoJsonPath = Path.Combine(settingsPath, "GameInfo.json");

                        var gameInfoSettings = Encoding.UTF8.GetString(Resources.GameInfo);

                        parsedArgs.GameInfoJson = gameInfoSettings;

                        File.WriteAllText(parsedArgs.GameInfoJsonPath, gameInfoSettings);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e);
                        return;
                    }
                }
            }

            var configGameServerSettings = GameServerConfig.Default();

            if (string.IsNullOrEmpty(parsedArgs.GameServerSettingsJson))
            {
                if (File.Exists(parsedArgs.GameServerSettingsJsonPath))
                {
                    configGameServerSettings = GameServerConfig.LoadFromFile(parsedArgs.GameServerSettingsJsonPath);
                }
                else
                {
                    try
                    {
                        Directory.CreateDirectory(settingsPath);

                        parsedArgs.GameServerSettingsJsonPath = Path.Combine(settingsPath, "GameServerSettings.json");

                        var gameServerSettings = Encoding.UTF8.GetString(Resources.GameServerSettings);

                        parsedArgs.GameServerSettingsJson = gameServerSettings;

                        File.WriteAllText(parsedArgs.GameServerSettingsJsonPath, gameServerSettings);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e);
                        return;
                    }
                }
            }
            else
            {
                configGameServerSettings = GameServerConfig.LoadFromJson(parsedArgs.GameServerSettingsJson);
            }

            var gameServerBlowFish = "17BLOhi6KZsTtldTsizvHg==";
            var gameServerLauncher = new GameServerLauncher(parsedArgs.ServerPort, parsedArgs.GameInfoJson, gameServerBlowFish);

#if DEBUG
            if (configGameServerSettings.AutoStartClient)
            {
                var leaguePath = configGameServerSettings.ClientLocation;
                if (Directory.Exists(leaguePath))
                {
                    leaguePath = Path.Combine(leaguePath, "League of Legends.exe");
                }
                if (File.Exists(leaguePath))
                {
                    var startInfo = new ProcessStartInfo(leaguePath)
                    {
                        Arguments = String.Format("\"8394\" \"LoLLauncher.exe\" \"\" \"127.0.0.1 {0} {1} 1\"",
                            parsedArgs.ServerPort, gameServerBlowFish),
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
                    _logger.Info("Unable to find League of Legends.exe. Check the GameServerSettings.json settings and your League location.");
                }
            }
            else
            {
                _logger.Info("Server is ready, clients can now connect.");
            }
#endif
            gameServerLauncher.StartNetworkLoop();
        }
    }

    public class ArgsOptions
    {
        [Option("config", DefaultValue = "Settings/GameInfo.json")]
        public string GameInfoJsonPath { get; set; }

        [Option("config-gameserver", DefaultValue = "Settings/GameServerSettings.json")]
        public string GameServerSettingsJsonPath { get; set; }

        [Option("config-json", DefaultValue = "")]
        public string GameInfoJson { get; set; }

        [Option("config-gameserver-json", DefaultValue = "")]
        public string GameServerSettingsJson { get; set; }

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
