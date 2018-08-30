﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            parsedArgs.GameInfoJson = LoadConfig(
                parsedArgs.GameInfoJsonPath,
                parsedArgs.GameInfoJson,
                Encoding.UTF8.GetString(Resources.GameInfo));

            var gameServerBlowFish = "17BLOhi6KZsTtldTsizvHg==";
            var gameServerLauncher = new GameServerLauncher(
                parsedArgs.ServerPort,
                parsedArgs.GameInfoJson,
                gameServerBlowFish);

#if DEBUG
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
                    var startInfo = new ProcessStartInfo(leaguePath)
                    {
                        Arguments = "\"8394\" \"LoLLauncher.exe\" \"\" " +
                                    $"\"127.0.0.1 {parsedArgs.ServerPort} {gameServerBlowFish} 1\"",
                        WorkingDirectory = Path.GetDirectoryName(leaguePath)
                    };

                    var leagueProcess = Process.Start(startInfo);

                    _logger.Info("Launching League of Legends. You can disable this in GameServerSettings.json.");

                    var windowsVersions = new[]
                    {
                        PlatformID.Win32NT,
                        PlatformID.Win32S,
                        PlatformID.Win32Windows,
                        PlatformID.WinCE
                    };

                    if (windowsVersions.Contains(Environment.OSVersion.Platform))
                    {
                        WindowsConsoleCloseDetection.SetCloseHandler(delegate
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
                    _logger.Info("Unable to find League of Legends.exe." +
                                 "Check the GameServerSettings.json settings and your League location."
                                );
                }
            }
            else
            {
                _logger.Info("Server is ready, clients can now connect.");
            }
#endif
            gameServerLauncher.StartNetworkLoop();
        }

        private static string LoadConfig(string filePath, string currentJsonString, string defaultJsonString)
        {
            if (!string.IsNullOrEmpty(currentJsonString))
            {
                return currentJsonString;
            }

            try
            {
                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath);
                }

                var settingsDirectory = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(settingsDirectory))
                {
                    throw new Exception($"Creating Config File failed. Invalid Path: {filePath}");
                }

                Directory.CreateDirectory(settingsDirectory);

                File.WriteAllText(filePath, defaultJsonString);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }

            return defaultJsonString;
        }
    }

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
            var options = new ArgsOptions();
            Parser.Default.ParseArguments<ArgsOptions>(args).WithParsed(o => options = o);
            return options;
        }
    }
}
