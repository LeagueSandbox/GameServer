using GameServerCore;
using GameServerCore.Enums;
using GameServerCore.Maps;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Chatbox;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Maps;
using LeagueSandbox.GameServer.Packets;
using LeagueSandbox.GameServer.Players;
using LeagueSandbox.GameServer.Scripting.CSharp;
using log4net;
using LeagueSandbox.GameServer.Items;
using PacketDefinitions420;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Timer = System.Timers.Timer;

namespace LeagueSandbox.GameServer
{
    public class Game : IGame
    {
        
        private ILog _logger;

        public bool IsRunning { get; private set; }
        public bool IsPaused { get; set; }

        private Timer _pauseTimer;
        public long PauseTimeLeft { get; private set; }
        private bool _autoResumeCheck;
        public bool SetToExit { get; set; }

        public int PlayersReady { get; private set; }

        public float GameTime { get; private set; }
        private float _nextSyncTime = 10 * 1000;

        private PacketServer _packetServer;
        public IPacketReader PacketReader { get; private set; }
        public IPacketNotifier PacketNotifier { get; private set; }
        public IObjectManager ObjectManager { get; private set; }
        public Map Map { get; private set; }
        
        public Config Config { get; protected set; }
        protected const double REFRESH_RATE = 1000.0 / 30.0; // 30 fps

        // Object managers
        internal ItemManager ItemManager { get; private set; }
        // Other managers
        internal ChatCommandManager ChatCommandManager { get; private set; }
        public IPlayerManager PlayerManager { get; private set; }
        internal NetworkIdManager NetworkIdManager { get; private set; }
        //Script Engine
        internal CSharpScriptEngine ScriptEngine { get; private set; }

        IMap IGame.Map => Map;

        private Stopwatch _lastMapDurationWatch;

        private List<GameScriptTimer> _gameScriptTimers;

        public Game(ItemManager itemManager)
        {
            _logger = LoggerProvider.GetLogger();
            ItemManager = itemManager;
            ChatCommandManager = new ChatCommandManager(this);
            NetworkIdManager = new NetworkIdManager();
            PlayerManager = new PlayerManager(this);
            ScriptEngine = new CSharpScriptEngine();
        }

        public void Initialize(ushort port, string blowfishKey, Config config)
        {
            _logger.Info("Loading Config.");
            Config = config;

            _gameScriptTimers = new List<GameScriptTimer>();

            ChatCommandManager.LoadCommands();

            ObjectManager = new ObjectManager(this);
            Map = new Map(this);
            ApiFunctionManager.SetGame(this);
            ApiEventManager.SetGame(this);
            IsRunning = false;

            _logger.Info("Loading C# Scripts");

            LoadScripts();

            Map.Init();

            _logger.Info("Add players");
            foreach (var p in Config.Players)
            {
                ((PlayerManager)PlayerManager).AddPlayer(p);
            }

            _pauseTimer = new Timer
            {
                AutoReset = true,
                Enabled = false,
                Interval = 1000
            };
            _pauseTimer.Elapsed += (sender, args) => PauseTimeLeft--;
            PauseTimeLeft = 30 * 60; // 30 minutes

            _packetServer = new PacketServer();
            _packetServer.InitServer(port, blowfishKey, this);
            PacketNotifier = new PacketNotifier(_packetServer.PacketHandlerManager, Map.NavGrid);
            // TODO: make lib to only get API types and not byte[], start from removing this line
            PacketReader = new PacketReader();

            _logger.Info("Game is ready.");
        }

        public bool LoadScripts()
        {
            return ScriptEngine.LoadSubdirectoryScripts($"{Config.ContentPath}/{Config.GameConfig.GameMode}/");
        }

        public void GameLoop()
        {
            _lastMapDurationWatch = new Stopwatch();
            _lastMapDurationWatch.Start();
            while (!SetToExit)
            {
                _packetServer.NetLoop();
                if (IsPaused)
                {
                    _lastMapDurationWatch.Stop();
                    _pauseTimer.Enabled = true;
                    if (PauseTimeLeft <= 0 && !_autoResumeCheck)
                    {
                        PacketNotifier.NotifyUnpauseGame();
                        _autoResumeCheck = true;
                    }
                    continue;
                }

                if (_lastMapDurationWatch.Elapsed.TotalMilliseconds + 1.0 > REFRESH_RATE)
                {
                    var sinceLastMapTime = _lastMapDurationWatch.Elapsed.TotalMilliseconds;
                    _lastMapDurationWatch.Restart();
                    if (IsRunning)
                    {
                        Update((float)sinceLastMapTime);

                    }
                }
                Thread.Sleep(1);
            }

        }
        public void Update(float diff)
        {
            GameTime += diff;
            ((ObjectManager)ObjectManager).Update(diff);
            Map.Update(diff);
            _gameScriptTimers.ForEach(gsTimer => gsTimer.Update(diff));
            _gameScriptTimers.RemoveAll(gsTimer => gsTimer.IsDead());

            // By default, synchronize the game time every 10 seconds
            _nextSyncTime += diff;
            if (_nextSyncTime >= 10 * 1000)
            {
                PacketNotifier.NotifyGameTimer(GameTime);
                _nextSyncTime = 0;
            }
        }

        public void AddGameScriptTimer(GameScriptTimer timer)
        {
            _gameScriptTimers.Add(timer);
        }

        public void RemoveGameScriptTimer(GameScriptTimer timer)
        {
            _gameScriptTimers.Remove(timer);
        }

        public void IncrementReadyPlayers()
        {
            PlayersReady++;
        }

        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void Pause()
        {
            if (PauseTimeLeft <= 0)
            {
                return;
            }
            IsPaused = true;
            PacketNotifier.NotifyPauseGame((int)PauseTimeLeft, true);
        }

        public void Unpause()
        {
            _lastMapDurationWatch.Start();
            IsPaused = false;
            _pauseTimer.Enabled = false;
        }

        public bool HandleDisconnect(int userId)
        {
            var peerinfo = PlayerManager.GetPeerInfo(userId);
            if (peerinfo != null)
            {
                if (!peerinfo.IsDisconnected)
                {
                    PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.SUMMONER_DISCONNECTED, peerinfo.Champion);
                }
                peerinfo.IsDisconnected = true;
            }
            return true;
        }

        // for reflection to work we need it to be called from lib
        public Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>> GetAllPacketHandlers()
        {
            var inst = GetInstances<PacketHandlerBase>(this);
            var dict = new Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>>();
            foreach (var pktCmd in inst)
            {
                dict.Add(pktCmd.PacketType, new Dictionary<Channel, IPacketHandler>
                {
                    {
                        pktCmd.PacketChannel, pktCmd
                    }
                });
            }
            return dict;
        }
        private static List<T> GetInstances<T>(IGame g)
        {
            return (Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(t => t.BaseType == (typeof(T)))
                .Select(t => (T)Activator.CreateInstance(t, g))).ToList();
        }

        public void SetGameToExit()
        {
            _logger.Info("Game is over. Game Server will exit in 10 seconds.");
            var timer = new Timer(10000) { AutoReset = false };
            timer.Elapsed += (a, b) => SetToExit = true;
            timer.Start();
        }
    }
}
