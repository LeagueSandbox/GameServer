using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ENet;
using LeagueSandbox.GameServer.Exceptions;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;
using Timer = System.Timers.Timer;

namespace LeagueSandbox.GameServer.Logic
{
    public class Game
    {
        private Host _server;
        public static BlowFish Blowfish { get; private set; }

        public static bool IsRunning { get; private set; }

        public static bool IsPaused { get; set; }
        private static Timer _pauseTimer;
        public static long PauseTimeLeft { get; private set; }
        private bool _autoResumeCheck;

        public static int PlayersReady { get; private set; }

        public static float GameTime { get; private set; }
        private float _nextSyncTime = 10 * 1000;


        public static ObjectManager ObjectManager { get; private set; }
        public static Map Map { get; private set; }
        public static PacketNotifier PacketNotifier { get; private set; }
        public static PacketHandlerManager PacketHandlerManager { get; private set; }
        public static Config Config { get; protected set; }
        protected const int PeerMTU = 996;
        protected const double RefreshRate = 1000.0 / 30.0; // 30 fps
        // Other managers
        private static Stopwatch _lastMapDurationWatch;

        private List<GameScriptTimer> _gameScriptTimers;

        public void Initialize(Address address, string blowfishKey, Config config)
        {
            Logger.LogCoreInfo("Loading Config.");
            Config = config;

            _gameScriptTimers = new List<GameScriptTimer>();

            ChatCommandManager.LoadCommands();
            _server = new Host();
            _server.Create(address, 32, 32, 0, 0);

            var key = Convert.FromBase64String(blowfishKey);
            if (key.Length <= 0)
            {
                throw new InvalidKeyException("Invalid blowfish key supplied");
            }

            Blowfish = new BlowFish(key);
            PacketHandlerManager = new PacketHandlerManager(Blowfish, _server);


            ObjectManager = new ObjectManager(this);
            Map = new Map(this);

            PacketNotifier = new PacketNotifier(this);
            ApiFunctionManager.SetGame(this);
            ApiEventManager.SetGame(this);
            IsRunning = false;

            Logger.LogCoreInfo("Loading C# Scripts");

            LoadScripts();

            Map.Init();

            foreach (var p in Config.Players)
            {
                PlayerManager.AddPlayer(p);
            }

            _pauseTimer = new Timer
            {
                AutoReset = true,
                Enabled = false,
                Interval = 1000
            };
            _pauseTimer.Elapsed += (sender, args) => PauseTimeLeft--;
            PauseTimeLeft = 30 * 60; // 30 minutes

            Logger.LogCoreInfo("Game is ready.");
        }

        public static bool LoadScripts()
        {
            return CSharpScriptEngine.LoadSubdirectoryScripts($"Content/Data/{Config.GameConfig.GameMode}/");
        }

        public void NetLoop()
        {
            _lastMapDurationWatch = new Stopwatch();
            _lastMapDurationWatch.Start();

            while (!Program.IsSetToExit)
            {
                while (_server.Service(0, out var enetEvent) > 0)
                {
                    switch (enetEvent.Type)
                    {
                        case EventType.Connect:
                            // Set some defaults
                            enetEvent.Peer.Mtu = PeerMTU;
                            enetEvent.Data = 0;
                            break;

                        case EventType.Receive:
                            var channel = (Channel)enetEvent.ChannelID;
                            PacketHandlerManager.HandlePacket(enetEvent.Peer, enetEvent.Packet, channel);
                            // Clean up the packet now that we're done using it.
                            enetEvent.Packet.Dispose();
                            break;

                        case EventType.Disconnect:
                            HandleDisconnect(enetEvent.Peer);
                            break;
                    }
                }

                if (IsPaused)
                {
                    _lastMapDurationWatch.Stop();
                    _pauseTimer.Enabled = true;
                    if (PauseTimeLeft <= 0 && !_autoResumeCheck)
                    {
                        PacketHandlerManager.GetHandler(PacketCmd.PKT_UNPAUSEGame, Channel.CHL_C2_S)
                            .HandlePacket(null, new byte[0]);
                        _autoResumeCheck = true;
                    }
                    continue;
                }

                if (_lastMapDurationWatch.Elapsed.TotalMilliseconds + 1.0 > RefreshRate)
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
            ObjectManager.Update(diff);
            Map.Update(diff);
            _gameScriptTimers.ForEach(gsTimer => gsTimer.Update(diff));
            _gameScriptTimers.RemoveAll(gsTimer => gsTimer.IsDead());

            // By default, synchronize the game time every 10 seconds
            _nextSyncTime += diff;
            if (_nextSyncTime >= 10 * 1000)
            {
                PacketNotifier.NotifyGameTimer();
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

        public static void IncrementReadyPlayers()
        {
            PlayersReady++;
        }

        public static void Start()
        {
            IsRunning = true;
        }

        public static void Stop()
        {
            IsRunning = false;
        }

        public static void Pause()
        {
            if (PauseTimeLeft <= 0)
            {
                return;
            }
            IsPaused = true;
            PacketNotifier.NotifyPauseGame((int)PauseTimeLeft, true);
        }

        public static void Unpause()
        {
            _lastMapDurationWatch.Start();
            IsPaused = false;
            _pauseTimer.Enabled = false;
        }

        private bool HandleDisconnect(Peer peer)
        {
            var peerinfo = PlayerManager.GetPeerInfo(peer);
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
    }
}
