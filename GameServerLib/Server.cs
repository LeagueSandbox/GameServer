using System;
using ENet;
using LeagueSandbox.GameServer.Logic;

namespace LeagueSandbox.GameServer
{
    internal class Server : IDisposable
    {
        private string _blowfishKey = "17BLOhi6KZsTtldTsizvHg==";
        private uint _serverHost = Address.IPv4HostAny;
        private ushort _serverPort = Program.ServerPort;
        private string _serverVersion = "0.2.0";
        private Game Game;
        private Config _config;

        public Server(Game game)
        {
            Game = game;
            _config = Config.LoadFromJson(Program.ConfigJson);
        }

        public void Start()
        {
            Logger.LogCoreInfo($"Yorick {_serverVersion}");
            Logger.LogCoreInfo("Game started on port: {0}", _serverPort);
            Game.Initialize(new Address(_serverHost, _serverPort), _blowfishKey, _config);
            Game.NetLoop();
        }

        public void Dispose()
        {
            // PathNode.DestroyTable();
        }
    }
}
