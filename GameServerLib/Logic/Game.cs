using BlowFishCS;
using ENet;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Exceptions;
using LeagueSandbox.GameServer.Logic;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Timer = System.Timers.Timer;

namespace LeagueSandbox.GameServer.Core.Logic
{
    public class Game
    {
        private Host _server;
        public BlowFish Blowfish { get; private set; }

        public bool IsRunning { get; private set; }

        public bool IsPaused { get; set; }
        private Timer _pauseTimer;
        public long PauseTimeLeft { get; private set; }
        private bool _autoResumeCheck;

        public int PlayersReady { get; private set; }

        public Map Map { get; private set; }
        public PacketNotifier PacketNotifier { get; private set; }
        public PacketHandlerManager PacketHandlerManager { get; private set; }
        public Config Config { get; protected set; }
        protected const int PEER_MTU = 996;
        protected const double REFRESH_RATE = 1000.0 / 30.0; // 30 fps
        private Logger _logger;
        // Object managers
        private ItemManager _itemManager;
        // Other managers
        private ChatCommandManager _chatCommandManager;
        private PlayerManager _playerManager;
        private NetworkIdManager _networkIdManager;
        private Stopwatch _lastMapDurationWatch;

        public Game(
            ItemManager itemManager,
            ChatCommandManager chatCommandManager,
            NetworkIdManager networkIdManager,
            PlayerManager playerManager,
            Logger logger
        )
        {
            _itemManager = itemManager;
            _chatCommandManager = chatCommandManager;
            _networkIdManager = networkIdManager;
            _playerManager = playerManager;
            _logger = logger;
        }

        public void Initialize(Address address, string blowfishKey, Config config)
        {
            _logger.LogCoreInfo("Loading Config.");
            Config = config;

            _chatCommandManager.LoadCommands();
            _server = new Host();
            _server.Create(address, 32, 32, 0, 0);

            var key = Convert.FromBase64String(blowfishKey);
            if (key.Length <= 0)
            {
                throw new InvalidKeyException("Invalid blowfish key supplied");
            }

            Blowfish = new BlowFish(key);
            PacketHandlerManager = new PacketHandlerManager(_logger, Blowfish, _server, _playerManager);

            RegisterMap((byte)Config.GameConfig.Map);

            PacketNotifier = new PacketNotifier(this, _playerManager, _networkIdManager);
            ApiFunctionManager.SetGame(this);
            IsRunning = false;

            foreach (var p in Config.Players)
            {
                _playerManager.AddPlayer(p);
            }

            _pauseTimer = new Timer
            {
                AutoReset = true,
                Enabled = false,
                Interval = 1000
            };
            _pauseTimer.Elapsed += (sender, args) => PauseTimeLeft--;
            PauseTimeLeft = 30 * 60; // 30 minutes

            _logger.LogCoreInfo("Game is ready.");
        }

        public void RegisterMap(byte mapId)
        {
            var mapName = $"{Config.ContentManager.GameModeName}-Map{mapId}";
            var dic = new Dictionary<string, Type>
            {
                { "LeagueSandbox-Default-Map1", typeof(SummonersRift) },
                { "LeagueSandbox-Default-Map4", typeof(OriginalTwistedTreeline) },
                // { "LeagueSandbox-Default-Map8", typeof(CrystalScar) },
                { "LeagueSandbox-Default-Map10", typeof(TwistedTreeline) },
                // { "LeagueSandbox-Default-Map11", typeof(NewSummonersRift) },
                { "LeagueSandbox-Default-Map12", typeof(HowlingAbyss) },
            };

            if (!dic.ContainsKey(mapName))
            {
                Map = new SummonersRift(this);
                return;
            }

            Map = (Map)Activator.CreateInstance(dic[mapName], this);
        }

        public void NetLoop()
        {
            var enetEvent = new Event();

            _lastMapDurationWatch = new Stopwatch();
            _lastMapDurationWatch.Start();
            using (PreciseTimer.SetResolution(1))
            {
                while (!Program.IsSetToExit)
                {
                    while (_server.Service(0, out enetEvent) > 0)
                    {
                        switch (enetEvent.Type)
                        {
                            case EventType.Connect:
                                // Set some defaults
                                enetEvent.Peer.Mtu = PEER_MTU;
                                enetEvent.Data = 0;
                                break;

                            case EventType.Receive:
                                PacketHandlerManager.handlePacket(enetEvent.Peer, enetEvent.Packet, (Channel)enetEvent.ChannelID);
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
                            PacketHandlerManager.GetHandler(PacketCmd.PKT_UnpauseGame, Channel.CHL_C2S)
                                .HandlePacket(null, new byte[0]);
                            _autoResumeCheck = true;
                        }
                        continue;
                    }

                    if (_lastMapDurationWatch.Elapsed.TotalMilliseconds + 1.0 > REFRESH_RATE)
                    {
                        double sinceLastMapTime = _lastMapDurationWatch.Elapsed.TotalMilliseconds;
                        _lastMapDurationWatch.Restart();
                        if (IsRunning)
                        {
                            Map.Update((float)sinceLastMapTime);
                        }
                    }
                    Thread.Sleep(1);
                }
            }
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

        private bool HandleDisconnect(Peer peer)
        {
            var peerinfo = _playerManager.GetPeerInfo(peer);
            if (peerinfo != null)
            {
                if (!peerinfo.IsDisconnected)
                {
                    PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.SummonerDisconnected, peerinfo.Champion);
                }
                peerinfo.IsDisconnected = true;
            }
            return true;
        }
    }
}
