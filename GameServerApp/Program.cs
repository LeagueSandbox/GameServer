using System;
using System.Diagnostics;
using System.IO;
using CommandLine;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Logic;
using LeagueSandbox.GameServerApp.Logic;
using LeagueSandbox.GameServerApp.Utility;

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

            var configGameServerSettingsJson = options.ConfigGameServerSettingsJson;
            GameServerConfig configGameServerSettings;
            if (string.IsNullOrEmpty(configGameServerSettingsJson))
            {
                configGameServerSettings = GameServerConfig.LoadFromFile(options.ConfigGameServerPath);
            } else
            {
                configGameServerSettings = GameServerConfig.Default();
            }

            string blowfishKey = "17BLOhi6KZsTtldTsizvHg==";
            var gameServerLauncher = new GameServerLauncher(options.ServerPort, configJson, blowfishKey, (Logger logger)=> {
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
                        logger.LogCoreInfo("Launching League of Legends. You can disable this in GameServerSettings.json.");
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT ||
                            Environment.OSVersion.Platform == PlatformID.Win32S ||
                            Environment.OSVersion.Platform == PlatformID.Win32Windows ||
                            Environment.OSVersion.Platform == PlatformID.WinCE)
                        {
                            WindowsConsoleCloseDetection.SetCloseHandler((_) => {
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
                        logger.LogCoreError("Unable to find League of Legends.exe. Check the GameServerSettings.json settings and your League location.");
                    }
                }
                #endif
            });
        }
    }

    public class ArgsOptions
    {
        [Option("config", DefaultValue = "Settings/GameInfo.json")]
        public string ConfigPath { get; set; }

        [Option("config-gameserver", DefaultValue = "Settings/GameServerSettings.json")]
        public string ConfigGameServerPath { get; set; }

        [Option("config-json", DefaultValue = "")]
        public string ConfigJson { get; set; }

        [Option("config-gameserver-json", DefaultValue = "")]
        public string ConfigGameServerSettingsJson { get; set; }

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
