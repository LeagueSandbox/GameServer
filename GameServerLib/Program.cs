using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using LeagueSandbox.GameServer.Logic;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Content;

namespace LeagueSandbox.GameServer
{
    /// <summary>
    /// This class is used by the GameServerApp to launch the server.
    /// Ideally the Program class in this project would be removed entirely, but that's not possible yet.
    /// </summary>
    public class GameServerLauncher
    {
        public static void LaunchServer(ushort serverPort, string configJson)
        {
            Program.Run(serverPort, configJson);
        }
    }

    internal class Program
    {
        // TODO: Require consumers of this inject a ServerContext
        public static string ExecutingDirectory { get; private set; }
        public static bool IsSetToExit { get; set; }
        public static string ConfigJson { get; private set; }
        public static ushort ServerPort { get; private set; }

        public static void Run(ushort serverPort, string configJson)
        {
            ConfigJson = configJson;
            ServerPort = serverPort;

            Logger.CreateLogger();
            
            var game = new Game();
            var server = new Server(game);

            try
            {
                ExecutingDirectory = ServerContext.ExecutingDirectory;
                ItemManager.LoadItems();
                server.Start();
            }
            catch (Exception e)
            {
                Logger.LogFatalError("Error: {0}", e.ToString());
#if DEBUG
                throw;
#endif
            }
        }

        public static void SetToExit()
        {
            Logger.LogCoreInfo("Game is over. Game Server will exit in 10 seconds.");
            var timer = new Timer(10000) { AutoReset = false };
            timer.Elapsed += (a, b) => IsSetToExit = true;
            timer.Start();
        }

        public static List<T> GetInstances<T>(Assembly a)
        {
            return (from t in Assembly.GetCallingAssembly().GetTypes()
                where t.BaseType == (typeof(T)) && t.GetConstructor(Type.EmptyTypes) != null
                select (T)Activator.CreateInstance(t)).ToList();
        }
    }
}
