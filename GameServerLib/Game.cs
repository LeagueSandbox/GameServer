﻿using GameServerCore;
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
using GameServerCore.Packets.PacketDefinitions;
using GameServerCore.Packets.PacketDefinitions.Requests;
using LeagueSandbox.GameServer.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer
{
    /// <summary>
    /// Class that contains and manages all qualities of the game such as managers for networking and game mechanics, as well as the starting, pausing, and stopping of the game.
    /// </summary>
    public class Game : IGame
    {
        // Crucial Game Vars
        private PacketServer _packetServer;
        private Stopwatch _lastMapDurationWatch;
        private List<GameScriptTimer> _gameScriptTimers;

        // Function Vars
        private ILog _logger;
        private Timer _pauseTimer;
        private bool _autoResumeCheck;
        private float _nextSyncTime = 10 * 1000;
        protected const double REFRESH_RATE = 1000.0 / 30.0; // 30 fps

        // Server

        /// <summary>
        /// Whether the server is running or not. Usually true after the network loop has started via GameServerLauncher.
        /// </summary>
        public bool IsRunning { get; private set; }
        /// <summary>
        /// Whether or not the game has been paused (via a chat command usually).
        /// </summary>
        public bool IsPaused { get; set; }
        /// <summary>
        /// Time until the game unpauses (if paused).
        /// </summary>
        public long PauseTimeLeft { get; private set; }
        /// <summary>
        /// Whether or not the game is set as finished (and thus whether the server should close).
        /// </summary>
        public bool SetToExit { get; set; }

        // Networking

        /// <summary>
        /// Number of players which have connected and are ready to be sent into the game (after fully loading).
        /// </summary>
        public int PlayersReady { get; private set; }
        /// <summary>
        /// Time since the game has started. Mostly used for networking to sync up players with the server.
        /// </summary>
        public float GameTime { get; private set; }
        /// <summary>
        /// Handler for request packets sent by game clients.
        /// </summary>
        public NetworkHandler<ICoreRequest> RequestHandler { get; }
        /// <summary>
        /// Handler for response packets sent by the server to game clients.
        /// </summary>
        public NetworkHandler<ICoreResponse> ResponseHandler { get; }
        /// <summary>
        /// Interface containing all function related packets (except handshake) which are sent by the server to game clients.
        /// </summary>
        public IPacketNotifier PacketNotifier { get; private set; }

        // Game

        /// <summary>
        /// Interface containing all (public) functions used by ObjectManager. ObjectManager manages GameObjects, their properties, and their interactions such as being added, removed, colliding with other objects or terrain, vision, teams, etc.
        /// </summary>
        public IObjectManager ObjectManager { get; private set; }
        /// <summary>
        /// Interface for all protection related functions.
        /// Protection is a mechanic which determines whether or not a unit is targetable.
        /// </summary>
        public IProtectionManager ProtectionManager { get; private set; }
        /// <summary>
        /// Interface for all map properties used for the game.
        /// </summary>
        public IMap Map { get; private set; }
        /// <summary>
        /// Class containing all information about the game's configuration such as game content location, map spawn points, whether cheat commands are enabled, etc.
        /// </summary>
        public Config Config { get; protected set; }
        /// <summary>
        /// Class which manages items of players.
        /// </summary>
        public ItemManager ItemManager { get; private set; }
        /// <summary>
        /// Class which manages all chat based commands.
        /// </summary>
        internal ChatCommandManager ChatCommandManager { get; private set; }
        /// <summary>
        /// Interface of functions used to identify players or their properties (such as their champion).
        /// </summary>
        public IPlayerManager PlayerManager { get; private set; }
        /// <summary>
        /// Manager for all unique identifiers used by GameObjects.
        /// </summary>
        internal NetworkIdManager NetworkIdManager { get; private set; }
        /// <summary>
        /// Class that compiles and loads all scripts which will be used for the game (ex: spells, items, AI, maps, etc).
        /// </summary>
        internal CSharpScriptEngine ScriptEngine { get; private set; }

        /// <summary>
        /// Instantiates all game managers and handlers.
        /// </summary>
        public Game()
        {
            _logger = LoggerProvider.GetLogger();
            ItemManager = new ItemManager();
            ChatCommandManager = new ChatCommandManager(this);
            NetworkIdManager = new NetworkIdManager();
            PlayerManager = new PlayerManager(this);
            ScriptEngine = new CSharpScriptEngine();
            RequestHandler = new NetworkHandler<ICoreRequest>();
            ResponseHandler = new NetworkHandler<ICoreResponse>();
        }

        /// <summary>
        /// Sets up all managers and config specific settings like players.
        /// </summary>
        /// <param name="config">Game configuration file. Usually from GameInfo.json.</param>
        /// <param name="server">Server networking instance.</param>
        public void Initialize(Config config, PacketServer server)
        {
            _logger.Info("Loading Config.");
            Config = config;

            _gameScriptTimers = new List<GameScriptTimer>();

            ChatCommandManager.LoadCommands();

            ObjectManager = new ObjectManager(this);
            ProtectionManager = new ProtectionManager(this);
            Map = new Map(this);
            ApiFunctionManager.SetGame(this);
            ApiEventManager.SetGame(this);
            IsRunning = false;

            Map.Init();

            _pauseTimer = new Timer
            {
                AutoReset = true,
                Enabled = false,
                Interval = 1000
            };
            _pauseTimer.Elapsed += (sender, args) => PauseTimeLeft--;
            PauseTimeLeft = 30 * 60; // 30 minutes

            // TODO: GameApp should send the Response/Request handlers
            _packetServer = server;
            // TODO: switch the notifier with ResponseHandler
            PacketNotifier = new PacketNotifier(_packetServer.PacketHandlerManager, Map.NavigationGrid);
            InitializePacketHandlers();

            _logger.Info("Add players");
            foreach (var p in Config.Players)
            {
                _logger.Info("Player " + p.Value.Name + " Added: " + p.Value.Champion);
                ((PlayerManager)PlayerManager).AddPlayer(p);
            }

            _logger.Info("Game is ready.");
        }

        /// <summary>
        /// Registers Request Handlers for each request packet.
        /// </summary>
        public void InitializePacketHandlers()
        {
            // maybe use reflection, the problem is that Register is generic and so it needs to know its type at 
            // compile time, maybe just use interface and in runetime figure out the type - and again there is
            // a problem with passing generic delegate to non-generic function, if we try to only constraint the
            // argument to interface ICoreRequest we will get an error cause our generic handlers use generic type
            // even with where statement that doesn't work
            RequestHandler.Register<AttentionPingRequest>(new HandleAttentionPing(this).HandlePacket);
            RequestHandler.Register<AutoAttackOptionRequest>(new HandleAutoAttackOption(this).HandlePacket);
            RequestHandler.Register<BlueTipClickedRequest>(new HandleBlueTipClicked(this).HandlePacket);
            RequestHandler.Register<BuyItemRequest>(new HandleBuyItem(this).HandlePacket);
            RequestHandler.Register<CastSpellRequest>(new HandleCastSpell(this).HandlePacket);
            RequestHandler.Register<ChatMessageRequest>(new HandleChatBoxMessage(this).HandlePacket);
            RequestHandler.Register<ClickRequest>(new HandleClick(this).HandlePacket);
            RequestHandler.Register<CursorPositionOnWorldRequest>(new HandleCursorPositionOnWorld(this).HandlePacket);
            RequestHandler.Register<EmotionPacketRequest>(new HandleEmotion(this).HandlePacket);
            RequestHandler.Register<ExitRequest>(new HandleExit(this).HandlePacket);
            RequestHandler.Register<HeartbeatRequest>(new HandleHeartBeat(this).HandlePacket);
            RequestHandler.Register<PingLoadInfoRequest>(new HandleLoadPing(this).HandlePacket);
            RequestHandler.Register<LockCameraRequest>(new HandleLockCamera(this).HandlePacket);
            RequestHandler.Register<MapRequest>(new HandleMap(this).HandlePacket);
            RequestHandler.Register<MovementRequest>(new HandleMove(this).HandlePacket);
            RequestHandler.Register<MoveConfirmRequest>(new HandleMoveConfirm(this).HandlePacket);
            RequestHandler.Register<PauseRequest>(new HandlePauseReq(this).HandlePacket);
            RequestHandler.Register<QueryStatusRequest>(new HandleQueryStatus(this).HandlePacket);
            RequestHandler.Register<QuestClickedRequest>(new HandleQuestClicked(this).HandlePacket);
            RequestHandler.Register<ScoreboardRequest>(new HandleScoreboard(this).HandlePacket);
            RequestHandler.Register<SellItemRequest>(new HandleSellItem(this).HandlePacket);
            RequestHandler.Register<SkillUpRequest>(new HandleSkillUp(this).HandlePacket);
            RequestHandler.Register<SpawnRequest>(new HandleSpawn(this).HandlePacket);
            RequestHandler.Register<StartGameRequest>(new HandleStartGame(this).HandlePacket);
            RequestHandler.Register<StatsConfirmRequest>(new HandleStatsConfirm(this).HandlePacket);
            RequestHandler.Register<SurrenderRequest>(new HandleSurrender(this).HandlePacket);
            RequestHandler.Register<SwapItemsRequest>(new HandleSwapItems(this).HandlePacket);
            RequestHandler.Register<SynchVersionRequest>(new HandleSync(this).HandlePacket);
            RequestHandler.Register<UnpauseRequest>(new HandleUnpauseReq(this).HandlePacket);
            RequestHandler.Register<UseObjectRequest>(new HandleUseObject(this).HandlePacket);
            RequestHandler.Register<ViewRequest>(new HandleView(this).HandlePacket);
        }

        /// <summary>
        /// Loads the scripts contained in every content package.
        /// </summary>
        /// <returns>Whether all scripts were loaded successfully or not.</returns>
        public bool LoadScripts()
        {
            var scriptLoadingResults = Config.ContentManager.LoadScripts();

            return scriptLoadingResults;
        }

        /// <summary>
        /// Function which initates ticking of the game's logic.
        /// </summary>
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

        /// <summary>
        /// Function called every tick of the game.
        /// </summary>
        /// <param name="diff">Number of milliseconds since this tick occurred.</param>
        public void Update(float diff)
        {
            GameTime += diff;
            ObjectManager.Update(diff);
            ProtectionManager.Update(diff);
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

        /// <summary>
        /// Adds a timer to the list of timers so that it ticks with the game.
        /// </summary>
        /// <param name="timer">Timer instance.</param>
        public void AddGameScriptTimer(GameScriptTimer timer)
        {
            _gameScriptTimers.Add(timer);
        }

        /// <summary>
        /// Removes a timer from the list of timers which causes it to become inactive.
        /// </summary>
        /// <param name="timer">Timer instance.</param>
        public void RemoveGameScriptTimer(GameScriptTimer timer)
        {
            _gameScriptTimers.Remove(timer);
        }

        /// <summary>
        /// Adds a player to the list of players who have fully loaded and are ready to get in-game.
        /// </summary>
        public void IncrementReadyPlayers()
        {
            PlayersReady++;
        }

        /// <summary>
        /// Function to set the game as running. Allows the game loop to start.
        /// </summary>
        public void Start()
        {
            IsRunning = true;
        }

        /// <summary>
        /// Function to set the game as not running. Prevents the game loop from continuing.
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
        }

        /// <summary>
        /// Temporarily prevents the game loop from continuing and notifies players.
        /// </summary>
        public void Pause()
        {
            if (PauseTimeLeft <= 0)
            {
                return;
            }
            IsPaused = true;
            PacketNotifier.NotifyPauseGame((int)PauseTimeLeft, true);
        }

        /// <summary>
        /// Releases the game loop from a temporary pause.
        /// </summary>
        public void Unpause()
        {
            _lastMapDurationWatch.Start();
            IsPaused = false;
            _pauseTimer.Enabled = false;
        }

        /// <summary>
        /// Unused function meant to get the instances of a specific type who rely on Game as a parameter.
        /// </summary>
        /// <returns>List of instances of type T.</returns>
        private static List<T> GetInstances<T>(IGame g)
        {
            return Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(t => t.BaseType == typeof(T))
                .Select(t => (T)Activator.CreateInstance(t, g)).ToList();
        }

        /// <summary>
        /// Prepares to close the Game 10 seconds after being called.
        /// </summary>
        public void SetGameToExit()
        {
            _logger.Info("Game is over. Game Server will exit in 10 seconds.");
            var timer = new Timer(10000) { AutoReset = false };
            timer.Elapsed += (a, b) => SetToExit = true;
            timer.Start();
        }
    }
}
