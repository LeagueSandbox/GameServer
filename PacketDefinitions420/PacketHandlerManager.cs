using LENet;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions;
using LeaguePackets;
using LeaguePackets.Game.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Channel = GameServerCore.Packets.Enums.Channel;

namespace PacketDefinitions420
{
    /// <summary>
    /// Class containing all functions related to sending and receiving packets.
    /// TODO: refactor this class (may be able to replace it with LeaguePackets' implementation), get rid of IGame, use generic API requests+responses also for disconnect and unpause
    /// </summary>
    public class PacketHandlerManager : IPacketHandlerManager
    {
        private delegate ICoreRequest RequestConvertor(byte[] data);
        private readonly Dictionary<Tuple<GamePacketID, Channel>, RequestConvertor> _gameConvertorTable;
        private readonly Dictionary<LoadScreenPacketID, RequestConvertor> _loadScreenConvertorTable;
        // should be one-to-one, no two users for the same Peer
        private readonly Dictionary<long, Peer> _peers;
        private readonly List<TeamId> _teamsEnumerator;
        private readonly IPlayerManager _playerManager;
        private readonly Dictionary<long, BlowFish> _blowfishes;
        private readonly Host _server;
        private readonly IGame _game;

        private readonly NetworkHandler<ICoreRequest> _netReq;
        private readonly NetworkHandler<ICoreRequest> _netResp;

        public PacketHandlerManager(Dictionary<long, BlowFish> blowfishes, Host server, IGame game, NetworkHandler<ICoreRequest> netReq, NetworkHandler<ICoreRequest> netResp)
        {
            _blowfishes = blowfishes;
            _server = server;
            _game = game;
            _peers = new Dictionary<long, Peer>();
            _teamsEnumerator = Enum.GetValues(typeof(TeamId)).Cast<TeamId>().ToList();
            _playerManager = _game.PlayerManager;
            _netReq = netReq;
            _netResp = netResp;
            _gameConvertorTable = new Dictionary<Tuple<GamePacketID, Channel>, RequestConvertor>();
            _loadScreenConvertorTable = new Dictionary<LoadScreenPacketID, RequestConvertor>();
            InitializePacketConvertors();
        }

        internal void InitializePacketConvertors()
        {
            foreach(var m in typeof(PacketReader).GetMethods())
            {
                foreach (Attribute attr in m.GetCustomAttributes(true))
                {
                    if (attr is PacketType)
                    {
                        if (((PacketType)attr).ChannelId == Channel.CHL_LOADING_SCREEN || ((PacketType)attr).ChannelId == Channel.CHL_COMMUNICATION)
                        {
                            var method = (RequestConvertor)Delegate.CreateDelegate(typeof(RequestConvertor), m);
                            _loadScreenConvertorTable.Add(((PacketType)attr).LoadScreenPacketId, method);
                        }
                        else
                        {
                            var key = new Tuple<GamePacketID, Channel>(((PacketType)attr).GamePacketId, ((PacketType)attr).ChannelId);
                            var method = (RequestConvertor)Delegate.CreateDelegate(typeof(RequestConvertor), m);
                            _gameConvertorTable.Add(key, method);
                        }
                    }
                }
            }
        }

        private RequestConvertor GetConvertor(LoadScreenPacketID packetId)
        {
            var packetsHandledWhilePaused = new List<LoadScreenPacketID>
            {
                LoadScreenPacketID.RequestJoinTeam,
                LoadScreenPacketID.Chat
            };

            if (_game.IsPaused && !packetsHandledWhilePaused.Contains(packetId))
            {
                return null;
            }

            if (_loadScreenConvertorTable.ContainsKey(packetId))
            {
                return _loadScreenConvertorTable[packetId];
            }

            return null;

        }

        private RequestConvertor GetConvertor(GamePacketID packetId, Channel channelId)
        {
            var packetsHandledWhilePaused = new List<GamePacketID>
            {
                GamePacketID.Dummy,
                GamePacketID.SynchSimTimeC2S,
                GamePacketID.ResumePacket,
                GamePacketID.C2S_QueryStatusReq,
                GamePacketID.C2S_ClientReady,
                GamePacketID.C2S_Exit,
                GamePacketID.World_SendGameNumber,
                GamePacketID.SendSelectedObjID,
                GamePacketID.C2S_CharSelected,

                // The next two are required to reconnect 
                GamePacketID.SynchVersionC2S,
                GamePacketID.C2S_Ping_Load_Info,

                // The next 5 are not really needed when reconnecting,
                // but they don't do much harm either
                GamePacketID.C2S_UpdateGameOptions,
                GamePacketID.OnReplication_Acc,
                GamePacketID.C2S_StatsUpdateReq,
                GamePacketID.World_SendCamera_Server,
                GamePacketID.C2S_OnTipEvent
            };
            if (_game.IsPaused && !packetsHandledWhilePaused.Contains(packetId))
            {
                return null;
            }
            var key = new Tuple<GamePacketID, Channel>(packetId, channelId);
            if (_gameConvertorTable.ContainsKey(key))
            {
                return _gameConvertorTable[key];
            }

            return null;
        }

        private void PrintPacket(byte[] buffer, string str)
        {
            // FIXME: currently lock disabled, not needed?
            Debug.Write(str);
            foreach (var b in buffer)
            {
                Debug.Write(b.ToString("X2") + " ");
            }

            Debug.WriteLine("");
            Debug.WriteLine("--------");
        }

        public bool SendPacket(int playerId, byte[] source, Channel channelNo, PacketFlags flag = PacketFlags.RELIABLE)
        {
            // Sometimes we try to send packets to a user that doesn't exist (like in broadcast when not all players are connected).
            // TODO: fix casting
            if (_peers.ContainsKey(playerId))
            {
                byte[] temp;
                if (source.Length >= 8)
                {
                    if (_blowfishes.ContainsKey(playerId))
                        temp = _blowfishes[playerId].Encrypt(source);
                    else
                        temp = source;
                }
                else
                {
                    temp = source;
                }

                return _peers[playerId].Send((byte)channelNo, new LENet.Packet(temp, flag)) == 0;
            }
            return false;
        }

        public bool BroadcastPacket(byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.RELIABLE)
        {
            if (data.Length >= 8)
            {
                // send packet to all peers and save failed ones
                List<KeyValuePair<long, Peer>> failedPeers = _peers.Where(x => x.Value.Send((byte)channelNo, new LENet.Packet(_blowfishes[x.Key].Encrypt(data), flag)) < 0).ToList();

                if (failedPeers.Count() > 0)
                {
                    Debug.WriteLine($"Broadcasting packet failed for {failedPeers.Count()} peers.");
                    return false;
                }
                return true;
            }
            else
            {
                var packet = new LENet.Packet(data, flag);
                _server.Broadcast((byte)channelNo, packet);
                return true;
            }
        }

        // TODO: find a way with no need of player manager
        public bool BroadcastPacketTeam(TeamId team, byte[] data, Channel channelNo,
            PacketFlags flag = PacketFlags.RELIABLE)
        {
            foreach (var ci in _playerManager.GetPlayers(true))
            {
                if (ci.Item2 != null && ci.Item2.Team == team)
                {
                    SendPacket((int)ci.Item2.PlayerId, data, channelNo, flag);
                }
            }

            return true;
        }

        public bool BroadcastPacketVision(IGameObject o, byte[] data, Channel channelNo,
            PacketFlags flag = PacketFlags.RELIABLE)
        {
            foreach (int pid in o.VisibleForPlayers)
            {
                SendPacket(pid, data, channelNo, flag);
            }
            return true;
        }

        public bool HandlePacket(Peer peer, byte[] data, Channel channelId)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            RequestConvertor convertor;

            if (channelId == Channel.CHL_COMMUNICATION || channelId == Channel.CHL_LOADING_SCREEN)
            {
                var loadScreenPacketId = (LoadScreenPacketID)reader.ReadByte();

                convertor = GetConvertor(loadScreenPacketId);
            }
            else
            {
                var gamePacketId = (GamePacketID)reader.ReadByte();

                convertor = GetConvertor(gamePacketId, channelId);

                switch (gamePacketId)
                {
                    case GamePacketID.World_SendCamera_Server:
                    case GamePacketID.Waypoint_Acc:
                    case GamePacketID.OnReplication_Acc:
                        break;
                }
            }

            reader.Close();

            if (convertor != null)
            {
                //TODO: improve dictionary reverse search
                long playerId = _peers.First(x => x.Value.Address.Equals(peer.Address)).Key;
                dynamic req = convertor(data);
                // TODO: fix all to use ulong
                _netReq.OnMessage((int)playerId, req);
                return true;
            }

            PrintPacket(data, "Error: ");
            return false;
        }

        public bool HandleDisconnect(Peer peer)
        {
            long playerId = _peers.FirstOrDefault(x => x.Value.Address.Equals(peer.Address)).Key;
            var player = _game.PlayerManager.GetPlayers().Find(x => x.Item2.PlayerId == playerId)?.Item2;
            if (player == null)
            {
                Debug.WriteLine($"prevented double disconnect of {playerId}");
                return true;
            }
            
            var peerInfo = _game.PlayerManager.GetPeerInfo(player.PlayerId);

            if (peerInfo != null)
            {
                var annoucement = new OnLeave { OtherNetID = peerInfo.Champion.NetId };
                _game.PacketNotifier.NotifyS2C_OnEventWorld(annoucement, peerInfo.Champion.NetId);
                peerInfo.IsDisconnected = true;
            }
            
            return player.Champion.OnDisconnect();
        }

        public bool HandlePacket(Peer peer, Packet packet, Channel channelId)
        {
            var data = packet.Data;

            // if channel id is HANDSHAKE we should initialize blowfish key and return
            if(channelId == Channel.CHL_HANDSHAKE)
            {
                return HandleHandshake(peer, data);
            }

            // every packet that is not blowfish go here
            if (data.Length >= 8)
            {
                long playerId = _peers.First(x => x.Value.Address.Equals(peer.Address)).Key;
                data = _blowfishes[playerId].Decrypt(data);
            }

            return HandlePacket(peer, data, channelId);
        }

        private bool HandleHandshake(Peer peer, byte[] data)
        {
            var request = PacketReader.ReadKeyCheckRequest(data);

            if (!_blowfishes.ContainsKey(request.PlayerID))
            {
                Debug.WriteLine($"Player ID {request.PlayerID} is invalid.");
                return false;
            }

            long playerID = _blowfishes[request.PlayerID].Decrypt(request.CheckSum);

            if(request.PlayerID != playerID)
            {
                Debug.WriteLine($"Blowfish key is wrong!");
                return false;
            }

            var peerInfo = _playerManager.GetPeerInfo(request.PlayerID);
            
            if(_peers.ContainsKey(request.PlayerID) && !peerInfo.IsDisconnected)
            {
                Debug.WriteLine($"Player {request.PlayerID} is already connected. Request from {peer.Address.ToString()}.");
                return false;
            }

            peerInfo.IsStartedClient = true;

            Debug.WriteLine("Connected player No " + request.PlayerID);      

            _peers[request.PlayerID] = peer;

            bool result = true;

            // inform players about their player numbers
            foreach (var player in _playerManager.GetPlayers(false))
            {
                var response = new KeyCheckPacket
                {
                    ClientID = (int)player.Item2.ClientId,
                    PlayerID = player.Item2.PlayerId,
                    //TODO: Unhardcode all values bellow
                    VersionNumber = 42000315,
                    Action = 0,
                    CheckSum = 0
                };
                // TODO: fix casting
                result = result && SendPacket((int)request.PlayerID, response.GetBytes(), Channel.CHL_HANDSHAKE);
            }

            // only if all packets were sent successfully return true
            return result;
        }
    }
}
