using ENet;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions;
using PacketDefinitions420.PacketDefinitions;
using PacketDefinitions420.PacketDefinitions.S2C;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace PacketDefinitions420
{
    // TODO: refactor this class, get rid of IGame, use generic API requests+responses also for disconnect and unpause
    public class PacketHandlerManager : IPacketHandlerManager
    {
        private delegate ICoreRequest RequestConvertor(byte[] data);
        private readonly Dictionary<Tuple<PacketCmd,Channel>, RequestConvertor> _convertorTable;
        // should be one-to-one, no two users for the same Peer
        private readonly Dictionary<int, Peer> _peers;
        private readonly Dictionary<int, int> _playerNum;
        private readonly List<TeamId> _teamsEnumerator;
        private readonly IPlayerManager _playerManager;
        private readonly BlowFish _blowfish;
        private readonly Host _server;
        private readonly IGame _game;

        private readonly NetworkHandler<ICoreRequest> _netReq;
        private readonly NetworkHandler<ICoreResponse> _netResp;

        private int _curPlayerNum = 0;

        public PacketHandlerManager(BlowFish blowfish, Host server, IGame game, NetworkHandler<ICoreRequest> netReq, NetworkHandler<ICoreResponse> netResp)
        {
            _blowfish = blowfish;
            _server = server;
            _game = game;
            _peers = new Dictionary<int, Peer>();
            _teamsEnumerator = Enum.GetValues(typeof(TeamId)).Cast<TeamId>().ToList();
            _playerNum = new Dictionary<int, int>();
            _playerManager = _game.PlayerManager;
            _netReq = netReq;
            _netResp = netResp;
            _convertorTable = new Dictionary<Tuple<PacketCmd, Channel>, RequestConvertor>();
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
                        var key = new Tuple<PacketCmd, Channel>(((PacketType)attr).PacketId, ((PacketType)attr).ChannelId);
                        var method = (RequestConvertor) Delegate.CreateDelegate(typeof(RequestConvertor), m);
                        _convertorTable.Add(key, method);
                    }
                }
            }
        }
        private RequestConvertor GetConvertor(PacketCmd cmd, Channel channelId)
        {
            var packetsHandledWhilePaused = new List<PacketCmd>
            {
                PacketCmd.PKT_UNPAUSE_GAME,
                PacketCmd.PKT_C2S_CHAR_LOADED,
                PacketCmd.PKT_C2S_CLICK,
                PacketCmd.PKT_C2S_CLIENT_READY,
                PacketCmd.PKT_C2S_EXIT,
                PacketCmd.PKT_C2S_HEART_BEAT,
                PacketCmd.PKT_C2S_QUERY_STATUS_REQ,
                PacketCmd.PKT_C2S_START_GAME,
                PacketCmd.PKT_C2S_WORLD_SEND_GAME_NUMBER,
                PacketCmd.PKT_CHAT_BOX_MESSAGE,
                PacketCmd.PKT_KEY_CHECK
            };
            if (_game.IsPaused && !packetsHandledWhilePaused.Contains(cmd))
            {
                return null;
            }
            var key = new Tuple<PacketCmd, Channel>(cmd, channelId);
            if (_convertorTable.ContainsKey(key))
            {
                return _convertorTable[key];
            }

            return null;
        }

        public bool SendPacket(int userId, Packet packet, Channel channelNo)
        {
            return SendPacket(userId, packet, channelNo, PacketFlags.Reliable);
        }

        public bool SendPacket(int userId, Packet packet, Channel channelNo, PacketFlags flags)
        {
            return SendPacket(userId, packet.GetBytes(), channelNo, flags);
        }

        public void UnpauseGame()
        {
            // FIXME: test this
            GetConvertor(PacketCmd.PKT_UNPAUSE_GAME, Channel.CHL_C2S)(new byte[0]);
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

        public bool SendPacket(int userId, byte[] source, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            byte[] temp;
            if (source.Length >= 8)
            {
                temp = _blowfish.Encrypt(source);
            }
            else
            {
                temp = source;
            }
            // sometimes we want to send packets to some user but this user doesn't exist (like in broadcast when not all players connected)
            if(_peers.ContainsKey(userId))
            {
                return _peers[userId].Send((byte)channelNo, temp);
            }
            return false;
        }

        public bool BroadcastPacket(byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            byte[] temp;
            if (data.Length >= 8)
            {
                temp = _blowfish.Encrypt(data);
            }
            else
            {
                temp = data;
            }

            var packet = new ENet.Packet();
            packet.Create(temp);
            _server.Broadcast((byte)channelNo, ref packet);
            return true;
        }

        public bool BroadcastPacket(Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return BroadcastPacket(packet.GetBytes(), channelNo, flag);
        }


        // TODO: find a way with no need of player manager
        public bool BroadcastPacketTeam(TeamId team, byte[] data, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            foreach (var ci in _playerManager.GetPlayers())
            {
                if (ci.Item2 != null && ci.Item2.Team == team)
                {
                    SendPacket((int)ci.Item2.UserId, data, channelNo, flag);
                }
            }

            return true;
        }

        public bool BroadcastPacketTeam(TeamId team, Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return BroadcastPacketTeam(team, packet.GetBytes(), channelNo, flag);
        }

        public bool BroadcastPacketVision(IGameObject o, Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return BroadcastPacketVision(o, packet.GetBytes(), channelNo, flag);
        }

        public bool BroadcastPacketVision(IGameObject o, byte[] data, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            foreach (var team in _teamsEnumerator)
            {
                if (team == TeamId.TEAM_NEUTRAL)
                {
                    continue;
                }

                if (_game.ObjectManager.TeamHasVisionOn(team, o))
                {
                    BroadcastPacketTeam(team, data, channelNo, flag);
                }
            }

            return true;
        }

        public bool HandlePacket(Peer peer, byte[] data, Channel channelId)
        {
            var header = new PacketHeader(data);
            var convertor = GetConvertor(header.Cmd, channelId);
            

            switch (header.Cmd)
            {
                case PacketCmd.PKT_C2S_STATS_CONFIRM:
                case PacketCmd.PKT_C2S_MOVE_CONFIRM:
                case PacketCmd.PKT_C2S_VIEW_REQ:
                    break;
            }

            if (convertor != null)
            {
                //TODO: improve dictionary reverse search
                int userId = _peers.First(x => x.Value.Address.Equals(peer.Address)).Key;
                dynamic req = convertor(data);             
                _netReq.OnMessage(userId, req);
                return true;
            }

            PrintPacket(data, "Error: ");
            return false;
        }
        public bool HandleDisconnect(Peer peer)
        {
            int userId = _peers.FirstOrDefault(x => x.Value == peer).Key;
            return _game.HandleDisconnect(userId);
        }
        public bool HandlePacket(Peer peer, ENet.Packet packet, Channel channelId)
        {
            var data = new byte[packet.Length];
            Marshal.Copy(packet.Data, data, 0, data.Length);

            // if channel id is HANDSHAKE we should initialize blowfish key and return
            if(channelId == Channel.CHL_HANDSHAKE)
            {
                return HandleHandshake(peer, data);
            }

            // every packet that is not blowfish go here
            if (data.Length >= 8)
            {
                // TODO: each user will have his unique key
                data = _blowfish.Decrypt(data);
            }
            return HandlePacket(peer, data, channelId);
        }
        private bool HandleHandshake(Peer peer, byte[] data)
        {
            var request = PacketReader.ReadKeyCheckRequest(data);
            // TODO: keys for every player
            int userId = (int)_blowfish.Decrypt(request.CheckId);

            if (request.UserId != userId)
            {
                // wrong blowfish key
                return false;
            }
            // TODO: removed code that return if player connected, check if there is no problem

            if (!_peers.ContainsKey(userId))
            {
                _playerNum[userId] = _curPlayerNum;
                _curPlayerNum++;
            }
            _peers[userId] = peer;

            bool result = true;
            // inform players about their player numbers
            foreach (var user in _peers.Keys.ToArray())
            {
                var response = new KeyCheckResponse(user, _playerNum[user]);
                result = result && SendPacket((int)request.UserId, response.GetBytes(), Channel.CHL_HANDSHAKE);
            }
            // only if all packets were sent successfully return true
            return result;
        }
    }
}
