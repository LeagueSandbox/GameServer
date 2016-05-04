using System;
using System.Collections.Generic;
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
using LeagueSandbox.GameServer.Logic.Content;

namespace LeagueSandbox.GameServer.Core.Logic
{
    public class Game
    {
        private Host _server;
        private BlowFish _blowfish;
        private uint _dwStart = 0x40000000; //new netid
        private object _lock = new object();

        private bool _started = false;
        private int _playersReady = 0;

        private List<Pair<uint, ClientInfo>> _players = new List<Pair<uint, ClientInfo>>();
        private Map _map;
        private PacketNotifier _packetNotifier;
        private PacketHandlerManager _packetHandlerManager;
        private System.Timers.Timer _updateTimer;
        private const int PEER_MTU = 996;
        private const PacketFlags RELIABLE = PacketFlags.Reliable;
        private const PacketFlags UNRELIABLE = PacketFlags.None;
        private const double REFRESH_RATE = 16.666; // 60 fps

        // Object managers
        public ItemManager ItemManager { get; protected set; }

        public bool Initialize(Address address, string baseKey)
        {
            ItemManager = ItemManager.LoadItems(this);

            _server = new Host();
            _server.Create(address, 32, 32, 0, 0);

            var key = System.Convert.FromBase64String(baseKey);
            if (key.Length <= 0)
                return false;

            _blowfish = new BlowFish(key);
            _packetHandlerManager = new PacketHandlerManager(this);
            _map = new SummonersRift(this);
            _packetNotifier = new PacketNotifier(this);

            var id = 1;
            foreach (var p in Config.Players)
            {
                var player = new ClientInfo(p.Value.Rank, ((p.Value.Team.ToLower() == "blue") ? TeamId.TEAM_BLUE : TeamId.TEAM_PURPLE), p.Value.Ribbon, p.Value.Icon);

                player.SetName(p.Value.Name);

                player.SetSkinNo(p.Value.Skin);
                player.UserId = id; // same as StartClient.bat
                id++;

                player.SetSummoners(StrToId(p.Value.Summoner1), StrToId(p.Value.Summoner2));

                var c = new Champion(this, p.Value.Champion, GetNewNetID(), (uint)player.UserId);
                var pos = c.getRespawnPosition();

                c.setPosition(pos.Item1, pos.Item2);
                c.setTeam((p.Value.Team.ToLower() == "blue") ? TeamId.TEAM_BLUE : TeamId.TEAM_PURPLE);
                c.LevelUp();

                player.SetChampion(c);
                var pair = new Pair<uint, ClientInfo>();
                pair.Item2 = player;
                _players.Add(pair);
            }
            return true;
        }
        public void NetLoop()
        {
            Update();

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
                            if (!_packetHandlerManager.handlePacket(enetEvent.Peer, enetEvent.Packet, (Channel)enetEvent.ChannelID))
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
                Thread.Sleep((int)REFRESH_RATE);
            }
        }

        private void Update()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _updateTimer = new System.Timers.Timer(REFRESH_RATE);
            _updateTimer.AutoReset = false;
            _updateTimer.Elapsed += (a, b) =>
                 {
                     watch.Stop();
                     var elapsed = watch.ElapsedMilliseconds;
                     watch.Restart();
                     if (_started)
                         _map.update(elapsed);

                     _updateTimer.Start();
                 };
            _updateTimer.Start();
        }

        public BlowFish GetBlowfish()
        {
            return _blowfish;
        }

        public Host GetServer()
        {
            return _server;
        }

        public List<Pair<uint, ClientInfo>> GetPlayers()
        {
            return _players;
        }

        public Map GetMap()
        {
            return _map;
        }

        public PacketNotifier GetPacketNotifier()
        {
            return _packetNotifier;
        }

        public PacketHandlerManager GetPacketHandlerManager()
        {
            return _packetHandlerManager;
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
            _updateTimer.Stop();
        }

        private bool HandleDisconnect(Peer peer)
        {
            var peerinfo = GetPeerInfo(peer);
            if (peerinfo != null)
            {
                // TODO: Handle disconnect
                Logger.LogCoreInfo("Player " + peerinfo.UserId + " disconnected");
            }
            return true;
        }

        public ClientInfo GetPeerInfo(Peer peer)
        {
            foreach (var player in _players)
                if (player.Item1 == peer.Address.port)
                    return player.Item2;
            return null;
        }

        private SummonerSpellIds StrToId(string str)
        {
            if (str == "FLASH")
            {
                return SummonerSpellIds.SPL_Flash;
            }
            else if (str == "IGNITE")
            {
                return SummonerSpellIds.SPL_Ignite;
            }
            else if (str == "HEAL")
            {
                return SummonerSpellIds.SPL_Heal;
            }
            else if (str == "BARRIER")
            {
                return SummonerSpellIds.SPL_Barrier;
            }
            else if (str == "SMITE")
            {
                return SummonerSpellIds.SPL_Smite;
            }
            else if (str == "GHOST")
            {
                return SummonerSpellIds.SPL_Ghost;
            }
            else if (str == "REVIVE")
            {
                return SummonerSpellIds.SPL_Revive;
            }
            else if (str == "CLEANSE")
            {
                return SummonerSpellIds.SPL_Cleanse;
            }
            else if (str == "TELEPORT")
            {
                return SummonerSpellIds.SPL_Teleport;
            }

            return 0;
        }

        public uint GetNewNetID()
        {
            lock (_lock)
            {
                _dwStart++;
                return _dwStart;
            }
        }
    }
}
