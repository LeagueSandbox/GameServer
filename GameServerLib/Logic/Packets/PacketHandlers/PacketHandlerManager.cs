﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ENet;
using GameServerCore.Logic.Domain.GameObjects;
using GameServerCore.Logic.Enums;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Players;
using Packet = GameServerCore.Packets.PacketDefinitions.Packet;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class PacketHandlerManager : IPacketHandlerManager
    {
        private readonly Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>> _handlerTable;
        private readonly List<TeamId> _teamsEnumerator;
        private readonly BlowFish _blowfish;
        private readonly Host _server;
        private readonly PlayerManager _playerManager;
        private readonly Game _game;

        public PacketHandlerManager(BlowFish blowfish, Host server, Game game)
        {
            _blowfish = blowfish;
            _server = server;
            _playerManager = game.PlayerManager;
            _game = game;
            _teamsEnumerator = Enum.GetValues(typeof(TeamId)).Cast<TeamId>().ToList();

            _handlerTable = GetAllPacketHandlers(ServerLibAssemblyDefiningType.Assembly);
        }

        private Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>> GetAllPacketHandlers(Assembly loadFrom)
        {
            var inst = GetInstances<PacketHandlerBase>(loadFrom, _game);
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

        private static List<T> GetInstances<T>(Assembly a, Game g)
        {
            return (Assembly.GetCallingAssembly()
                .GetTypes()
                .Where(t => t.BaseType == (typeof(T)))
                .Select(t => (T)Activator.CreateInstance(t, g))).ToList();
        }

        internal IPacketHandler GetHandler(PacketCmd cmd, Channel channelId)
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
            if (_handlerTable.ContainsKey(cmd))
            {
                var handlers = _handlerTable[cmd];
                if (handlers.ContainsKey(channelId))
                {
                    return handlers[channelId];
                }
            }

            return null;
        }

        public bool SendPacket(Peer peer, Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return SendPacket(peer, packet.GetBytes(), channelNo, flag);
        }

        public void UnpauseGame()
        {
            GetHandler(PacketCmd.PKT_UNPAUSE_GAME, Channel.CHL_C2S)
                .HandlePacket(null, new byte[0]);
        }

        private void PrintPacket(byte[] buffer, string str)
        {
            //string hex = BitConverter.ToString(buffer);
            // System.Diagnostics.Debug.WriteLine(str + hex.Replace("-", " "));
            lock (ServerContext.ExecutingDirectory)
            {
                Debug.Write(str);
                foreach (var b in buffer)
                {
                    Debug.Write(b.ToString("X2") + " ");
                }

                Debug.WriteLine("");
                Debug.WriteLine("--------");
            }
        }

        public bool SendPacket(Peer peer, byte[] source, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            ////PDEBUG_LOG_LINE(Logging," Sending packet:\n");
            //if(length < 300)
            //printPacket(source, "Sent: ");
            byte[] temp;
            if (source.Length >= 8)
            {
                temp = _blowfish.Encrypt(source);
            }
            else
            {
                temp = source;
            }

            return peer.Send((byte)channelNo, temp);
        }

        public bool BroadcastPacket(byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            ////PDEBUG_LOG_LINE(Logging," Broadcast packet:\n");
            //printPacket(data, "Broadcast: ");
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


        public bool BroadcastPacketTeam(TeamId team, byte[] data, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            foreach (var ci in _playerManager.GetPlayers())
            {
                if (ci.Item2.Peer != null && ci.Item2.Team == team)
                {
                    SendPacket(ci.Item2.Peer, data, channelNo, flag);
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

                if (_game.ObjectManager.TeamHasVisionOn(team, (GameObject)o))
                {
                    BroadcastPacketTeam(team, data, channelNo, flag);
                }
            }

            return true;
        }

        public bool HandlePacket(Peer peer, byte[] data, Channel channelId)
        {
            var header = new PacketHeader(data);
            var handler = GetHandler(header.Cmd, channelId);

            switch (header.Cmd)
            {
                case PacketCmd.PKT_C2S_STATS_CONFIRM:
                case PacketCmd.PKT_C2S_MOVE_CONFIRM:
                case PacketCmd.PKT_C2S_VIEW_REQ:
                    break;
            }

            if (handler != null)
            {
                return handler.HandlePacket(peer, data);
            }

            PrintPacket(data, "Error: ");
            return false;
        }

        public bool HandlePacket(Peer peer, ENet.Packet packet, Channel channelId)
        {
            var data = new byte[packet.Length];
            Marshal.Copy(packet.Data, data, 0, data.Length);

            if (data.Length >= 8 && _playerManager.GetPeerInfo(peer) != null)
            {
                data = _blowfish.Decrypt(data);
            }

            return HandlePacket(peer, data, channelId);
        }
    }
}
