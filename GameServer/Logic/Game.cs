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
using static ENet.Native;
using System.Threading;
using LeagueSandbox.GameServer.Logic.Content;

namespace LeagueSandbox.GameServer.Core.Logic
{
    public unsafe class Game
    {
        private ENetHost* _server;
        private BlowFish* _blowfish;
        private static uint dwStart = 0x40000000; //new netid

        private bool _started = false;
        private int playersReady = 0;

        private List<Pair<uint, ClientInfo>> players = new List<Pair<uint, ClientInfo>>();
        private Map map;
        private System.Timers.Timer updateTimer;
        private const int PEER_MTU = 996;
        private const PacketFlags RELIABLE = PacketFlags.Reliable;
        private const PacketFlags UNRELIABLE = PacketFlags.None;
        private const double REFRESH_RATE = 16.666; // 60 fps

        // Object managers
        public ItemManager ItemManager { get; protected set; }

        public bool initialize(ENetAddress address, string baseKey)
        {
            ItemManager = ItemManager.LoadItems(this);
            if (enet_initialize() < 0)
                return false;

            _server = enet_host_create(&address, new IntPtr(32), new IntPtr(32), 0, 0);
            if (_server == null)
                return false;

            var key = System.Convert.FromBase64String(baseKey);

            if (key.Length <= 0)
                return false;

            fixed (byte* s = key)
            {
                _blowfish = BlowFishCS.BlowFishCS.BlowFishCreate(s, new IntPtr(16));
            }

            PacketHandlerManager.getInstace().InitHandlers(this);

            map = new SummonersRift(this);

            PacketNotifier.setMap(map);
            //TODO: better lua implementation

            var id = 1;
            foreach (var p in Config.players)
            {
                var player = new ClientInfo(p.Value.rank, ((p.Value.team.ToLower() == "blue") ? TeamId.TEAM_BLUE : TeamId.TEAM_PURPLE), p.Value.ribbon, p.Value.icon);

                player.setName(p.Value.name);

                player.setSkinNo(p.Value.skin);
                player.userId = id; // same as StartClient.bat
                id++;

                player.setSummoners(strToId(p.Value.summoner1), strToId(p.Value.summoner2));

                Champion c = ChampionFactory.getChampionFromType(this, p.Value.champion, map, GetNewNetID(), (uint)player.userId);
                var pos = c.getRespawnPosition();

                c.setPosition(pos.Item1, pos.Item2);
                c.setTeam((p.Value.team.ToLower() == "blue") ? TeamId.TEAM_BLUE : TeamId.TEAM_PURPLE);
                c.levelUp();

                player.setChampion(c);
                var pair = new Pair<uint, ClientInfo>();
                pair.Item2 = player;
                players.Add(pair);
            }
            return true;
        }
        public void netLoop()
        {
            update();

            var enetEvent = new ENetEvent();
            while (true)
            {
                while (enet_host_service(_server, &enetEvent, 0) > 0)
                {
                    switch (enetEvent.type)
                    {
                        case EventType.Connect:
                            //Logging->writeLine("A new client connected: %i.%i.%i.%i:%i", event.peer->address.host & 0xFF, (event.peer->address.host >> 8) & 0xFF, (event.peer->address.host >> 16) & 0xFF, (event.peer->address.host >> 24) & 0xFF, event.peer->address.port);

                            /* Set some defaults */
                            enetEvent.peer->mtu = PEER_MTU;
                            enetEvent.data = 0;
                            break;

                        case EventType.Receive:
                            if (!PacketHandlerManager.getInstace().handlePacket(enetEvent.peer, enetEvent.packet, (Channel)enetEvent.channelID))
                            {
                                //enet_peer_disconnect(event.peer, 0);
                            }

                            /* Clean up the packet now that we're done using it. */
                            enet_packet_destroy(enetEvent.packet);
                            break;

                        case EventType.Disconnect:
                            handleDisconnect(enetEvent.peer);
                            break;
                    }
                }
                Thread.Sleep((int)REFRESH_RATE);
            }
        }

        private void update()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            updateTimer = new System.Timers.Timer(REFRESH_RATE);
            updateTimer.AutoReset = false;
            updateTimer.Elapsed += (a, b) =>
                 {
                     watch.Stop();
                     var elapsed = watch.ElapsedMilliseconds;
                     watch.Restart();
                     if (_started)
                         map.update(elapsed);

                     updateTimer.Start();
                 };
            updateTimer.Start();
        }

        public BlowFish* getBlowfish()
        {
            return _blowfish;
        }

        public ENetHost* getServer()
        {
            return _server;
        }

        public List<Pair<uint, ClientInfo>> getPlayers()
        {
            return players;
        }

        public Map getMap()
        {
            return map;
        }

        public void IncrementReadyPlayers()
        {
            playersReady++;
        }

        public int getReadyPlayers()
        {
            return playersReady;
        }

        public bool isStarted()
        {
            return _started;
        }

        public void setStarted(bool b)
        {
            _started = b;
        }

        public void stopGame()
        {
            updateTimer.Stop();
        }

        bool handleDisconnect(ENetPeer* peer)
        {
            var peerinfo = getPeerInfo(peer);
            if (peerinfo != null)
            {
                // TODO: Handle disconnect
                Logger.LogCoreInfo("Player " + peerinfo.userId + " disconnected");
            }
            return true;
        }

        public ClientInfo getPeerInfo(ENetPeer* peer)
        {
            foreach (var player in players)
                if (player.Item1 == peer->address.port)
                    return player.Item2;
            return null;
        }

        SummonerSpellIds strToId(string str)
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

        public static uint GetNewNetID()
        {
            dwStart++;
            return dwStart;
        }
    }
}
