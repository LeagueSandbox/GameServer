using static ENet.Native;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Items;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeagueSandbox
{
    class Program
    {
        private static uint SERVER_HOST = ENET_HOST_ANY;
        private static ushort SERVER_PORT = 5119;
        private static string SERVER_KEY = "17BLOhi6KZsTtldTsizvHg==";
        private static string SERVER_VERSION = "0.2.0";
        public static string ExecutingDirectory;

        static void Main(string[] args)
        {
            Console.WriteLine("Yorick " + SERVER_VERSION);
            WriteToLog.ExecutingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            WriteToLog.LogfileName = "LeagueSandbox.txt";
            WriteToLog.CreateLogFile();
            ExecutingDirectory = WriteToLog.ExecutingDirectory;

            System.AppDomain.CurrentDomain.FirstChanceException += Logger.CurrentDomain_FirstChanceException;
            System.AppDomain.CurrentDomain.UnhandledException += Logger.CurrentDomain_UnhandledException;


            Logger.LogCoreInfo("Loading Config.");
            Config.LoadConfig("Settings/GameInfo.json");

            Logger.LogCoreInfo("Loading RAF files in filearchives/.");

            var settings = Settings.Load("Settings/Settings.json");
            if (!RAFManager.getInstance().init(System.IO.Path.Combine(settings.RadsPath, "filearchives")))
            {
                Logger.LogCoreError("Couldn't load RAF files. Make sure you have a 'filearchives' directory in the server's root directory. This directory is to be taken from RADS/projects/lol_game_client/");
                return;
            }

            Logger.LogCoreInfo("Game started");

            Game g = new Game();
            var address = new ENetAddress();
            address.host = SERVER_HOST;
            address.port = SERVER_PORT;


            if (!g.initialize(address, SERVER_KEY))
            {
                Logger.LogCoreError("Couldn't listen on port " + SERVER_PORT + ", or invalid key");
                return;
            }

            g.netLoop();

            PathNode.DestroyTable(); // Cleanup
        }
    }
}
