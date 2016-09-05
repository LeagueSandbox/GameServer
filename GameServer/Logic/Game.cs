using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using LeagueSandbox.GameServer.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.GameObjects;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Maps;
using System.Net.Sockets;
using System.Net;
using BlowFishCS;
using System.Threading;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Chatbox;
using Ninject;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic
{
    public class Game
    {
        protected Host _server;
        protected BlowFish _blowfish;
        protected object _lock = new object();

        protected bool _started = false;
        protected int _playersReady = 0;

        private Map _map;
        public PacketNotifier PacketNotifier;
        public PacketHandlerManager PacketHandlerManager;
        public Config Config { get; protected set; }
        protected const int PEER_MTU = 996;
        protected const PacketFlags RELIABLE = PacketFlags.Reliable;
        protected const PacketFlags UNRELIABLE = PacketFlags.None;
        protected const double REFRESH_RATE = 16.666; // 60 fps
        protected long _timeElapsed;

        private Logger _logger;
        // Object managers
        private ItemManager _itemManager;
        // Other managers
        private ChatboxManager _chatboxManager;
        private PlayerManager _playerManager;
        private NetworkIdManager _networkIdManager;

        public Game(
            ItemManager itemManager, 
            ChatboxManager chatboxManager,
            NetworkIdManager networkIdManager,
            PlayerManager playerManager,
            Logger logger
        ) {
            _itemManager = itemManager;
            _chatboxManager = chatboxManager;
            _networkIdManager = networkIdManager;
            _playerManager = playerManager;
            _logger = logger;
        }        

        public bool Initialize(Address address, string baseKey)
        {
            _logger.LogCoreInfo("Loading Config.");
            Config = new Config("Settings/GameInfo.json");

            _server = new Host();
            _server.Create(address, 32, 32, 0, 0);

            var key = System.Convert.FromBase64String(baseKey);
            if (key.Length <= 0)
                return false;

            _blowfish = new BlowFish(key);
            PacketHandlerManager = new PacketHandlerManager(_logger, _blowfish, _server, _playerManager);
            _map = new SummonersRift(this);
            PacketNotifier = new PacketNotifier(this, _playerManager, _networkIdManager);
            ApiFunctionManager.SetGame(this);

            foreach (var p in Config.Players)
            {
                _playerManager.AddPlayer(p);
            }
            return true;
        }

        public void NetLoop()
        {
            var watch = new Stopwatch();
            var enetEvent = new Event();
            while (true)
            {
                while (_server.Service(0, out enetEvent) > 0)
                {
                    switch (enetEvent.Type)
                    {
                        case EventType.Connect:
                            //Logging->writeLine("A new client connected: %i.%i.%i.%i:%i", event.peer->address.host & 0xFF, (event.peer->address.host >> 8) & 0xFF, (event.peer->address.host >> 16) & 0xFF, (event.peer->address.host >> 24) & 0xFF, event.peer->address.port);

                            /* Set some defaults */
                            enetEvent.Peer.Mtu = PEER_MTU;
                            enetEvent.Data = 0;
                            break;

                        case EventType.Receive:
                            if (!PacketHandlerManager.handlePacket(enetEvent.Peer, enetEvent.Packet, (Channel)enetEvent.ChannelID))
                            {
                                //enet_peer_disconnect(event.peer, 0);
                            }

                            /* Clean up the packet now that we're done using it. */
                            enetEvent.Packet.Dispose();
                            break;

                        case EventType.Disconnect:
                            HandleDisconnect(enetEvent.Peer);
                            break;
                    }
                }
                if (_started)
                    _map.Update(_timeElapsed);
                watch.Stop();
                _timeElapsed = watch.ElapsedMilliseconds;
                watch.Restart();
                var timeToWait = REFRESH_RATE - _timeElapsed;
                if (timeToWait < 0)
                {
                    timeToWait = 0;
                }
                Thread.Sleep((int)timeToWait);
            }
        }

        public BlowFish GetBlowfish()
        {
            return _blowfish;
        }

        public Host GetServer()
        {
            return _server;
        }

        public Map GetMap()
        {
            return _map;
        }

        public void IncrementReadyPlayers()
        {
            _playersReady++;
        }

        public int GetReadyPlayers()
        {
            return _playersReady;
        }

        public bool IsStarted()
        {
            return _started;
        }

        public void SetStarted(bool b)
        {
            _started = b;
        }

        public void StopGame()
        {
            _started = false;
        }

        private bool HandleDisconnect(Peer peer)
        {
            var peerinfo = _playerManager.GetPeerInfo(peer);
            if (peerinfo != null)
            {
                // TODO: Handle disconnect
                _logger.LogCoreInfo("Player " + peerinfo.UserId + " disconnected");
            }
            return true;
        }
    }
}
