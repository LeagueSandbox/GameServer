﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using System.Runtime.InteropServices;
using ENet;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic;
using BlowFishCS;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic
{
    public class PacketHandlerManager
    {
        private Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>> _handlerTable;
        private List<TeamId> _teamsEnumerator;
        private Logger _logger;
        private BlowFish _blowfish;
        private Host _server;
        private PlayerManager _playerManager;

        public PacketHandlerManager(Logger logger, BlowFish blowfish, Host server, PlayerManager playerManager)
        {
            _logger = logger;
            _blowfish = blowfish;
            _server = server;
            _playerManager = playerManager;
            _teamsEnumerator = Enum.GetValues(typeof(TeamId)).Cast<TeamId>().ToList();

            InitHandlers();
        }

        private void InitHandlers()
        {
            _handlerTable = new Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>>();

            registerHandler(new HandleKeyCheck(), PacketCmd.PKT_KeyCheck, Channel.CHL_HANDSHAKE);
            registerHandler(new HandleLoadPing(), PacketCmd.PKT_C2S_Ping_Load_Info, Channel.CHL_C2S);
            registerHandler(new HandleSpawn(), PacketCmd.PKT_C2S_CharLoaded, Channel.CHL_C2S);
            registerHandler(new HandleMap(), PacketCmd.PKT_C2S_ClientReady, Channel.CHL_LOADING_SCREEN);
            registerHandler(new HandleSynch(), PacketCmd.PKT_C2S_SynchVersion, Channel.CHL_C2S);
            registerHandler(new HandleCastSpell(), PacketCmd.PKT_C2S_CastSpell, Channel.CHL_C2S);
            // registerHandler(new HandleGameNumber(), PacketCmd.PKT_C2S_GameNumberReq, Channel.CHL_C2S);
            registerHandler(new HandleQueryStatus(), PacketCmd.PKT_C2S_QueryStatusReq, Channel.CHL_C2S);
            registerHandler(new HandleStartGame(), PacketCmd.PKT_C2S_StartGame, Channel.CHL_C2S);
            registerHandler(new HandleExit(), PacketCmd.PKT_C2S_Exit, Channel.CHL_C2S);
            registerHandler(new HandleView(), PacketCmd.PKT_C2S_ViewReq, Channel.CHL_C2S);
            registerHandler(new HandleNull(), PacketCmd.PKT_C2S_Click, Channel.CHL_C2S);
            // registerHandler(new HandleNull(), PacketCmd.PKT_C2S_OpenShop, Channel.CHL_C2S);
            registerHandler(new HandleAttentionPing(), PacketCmd.PKT_C2S_AttentionPing, Channel.CHL_C2S);
            registerHandler(new HandleChatBoxMessage(), PacketCmd.PKT_C2S_ChatBoxMessage, Channel.CHL_COMMUNICATION);
            registerHandler(new HandleMove(), PacketCmd.PKT_C2S_MoveReq, Channel.CHL_C2S);
            registerHandler(new HandleNull(), PacketCmd.PKT_C2S_MoveConfirm, Channel.CHL_C2S);
            registerHandler(new HandleSkillUp(), PacketCmd.PKT_C2S_SkillUp, Channel.CHL_C2S);
            registerHandler(new HandleEmotion(), PacketCmd.PKT_C2S_Emotion, Channel.CHL_C2S);
            registerHandler(new HandleBuyItem(), PacketCmd.PKT_C2S_BuyItemReq, Channel.CHL_C2S);
            registerHandler(new HandleSellItem(), PacketCmd.PKT_C2S_SellItem, Channel.CHL_C2S);
            registerHandler(new HandleSwapItems(), PacketCmd.PKT_C2S_SwapItems, Channel.CHL_C2S);
            registerHandler(new HandleNull(), PacketCmd.PKT_C2S_LockCamera, Channel.CHL_C2S);
            registerHandler(new HandleNull(), PacketCmd.PKT_C2S_StatsConfirm, Channel.CHL_C2S);
            registerHandler(new HandleClick(), PacketCmd.PKT_C2S_Click, Channel.CHL_C2S);
            registerHandler(new HandleHeartBeat(), PacketCmd.PKT_C2S_HeartBeat, Channel.CHL_GAMEPLAY);
            registerHandler(new HandleSurrender(), PacketCmd.PKT_C2S_Surrender, Channel.CHL_C2S);
            registerHandler(new HandleBlueTipClicked(), PacketCmd.PKT_C2S_BlueTipClicked, Channel.CHL_C2S);
            registerHandler(new HandleAutoAttackOption(), PacketCmd.PKT_C2S_AutoAttackOption, Channel.CHL_C2S);
            registerHandler(new HandleQuestClicked(), PacketCmd.PKT_C2S_QuestClicked, Channel.CHL_C2S);
            registerHandler(new HandleUseObject(), PacketCmd.PKT_C2S_UseObject, Channel.CHL_C2S);
            registerHandler(new HandleCursorPositionOnWorld(), PacketCmd.PKT_C2S_CursorPositionOnWorld, Channel.CHL_C2S);
            registerHandler(new HandleScoreboard(), PacketCmd.PKT_C2S_Scoreboard, Channel.CHL_C2S);

            // registerHandler(new HandleX(), PacketCmd.PKT_C2S_X, Channel.CHL_C2S);
        }

        public void registerHandler(IPacketHandler handler, PacketCmd pktcmd, Channel channel)
        {

            if (!_handlerTable.ContainsKey(pktcmd))
                _handlerTable.Add(pktcmd, new Dictionary<Channel, IPacketHandler>());

            var dict = _handlerTable[pktcmd];
            if (!dict.ContainsKey(channel))
                dict.Add(channel, handler);
            else
                dict[channel] = handler;
        }

        internal IPacketHandler GetHandler(PacketCmd cmd, Channel channelID)
        {
            if (_handlerTable.ContainsKey(cmd))
            {
                var handlers = _handlerTable[cmd];
                if (handlers.ContainsKey(channelID))
                    return handlers[channelID];
            }
            return null;
        }
        public bool sendPacket(Peer peer, GameServer.Logic.Packets.Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            return sendPacket(peer, packet.GetBytes(), channelNo, flag);
        }

        private IntPtr allocMemory(byte[] data)
        {
            var unmanagedPointer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, unmanagedPointer, data.Length);
            return unmanagedPointer;
        }

        private void releaseMemory(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }

        public void printPacket(byte[] buffer, string str)
        {
            //string hex = BitConverter.ToString(buffer);
            // System.Diagnostics.Debug.WriteLine(str + hex.Replace("-", " "));
            lock (Program.ExecutingDirectory)
            {
                System.Diagnostics.Debug.Write(str);
                foreach (var b in buffer)
                    System.Diagnostics.Debug.Write(b.ToString("X2") + " ");

                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("--------");
            }
        }
        public bool sendPacket(Peer peer, byte[] source, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            ////PDEBUG_LOG_LINE(Logging," Sending packet:\n");
            //if(length < 300)
            //printPacket(source, "Sent: ");
            byte[] temp;
            if (source.Length >= 8)
                temp = _blowfish.Encrypt(source);
            else
                temp = source;

            return peer.Send((byte)channelNo, temp);
        }

        public bool broadcastPacket(byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            ////PDEBUG_LOG_LINE(Logging," Broadcast packet:\n");
            //printPacket(data, "Broadcast: ");
            byte[] temp;
            if (data.Length >= 8)
                temp = _blowfish.Encrypt(data);
            else
                temp = data;

            var packet = new Packet();
            packet.Create(temp);
            _server.Broadcast((byte)channelNo, ref packet);
            return true;
        }

        public bool broadcastPacket(GameServer.Logic.Packets.Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            return broadcastPacket(packet.GetBytes(), channelNo, flag);
        }


        public bool broadcastPacketTeam(TeamId team, byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            foreach (var ci in _playerManager.GetPlayers())
                if (ci.Item2.Peer != null && ci.Item2.Team == team)
                    sendPacket(ci.Item2.Peer, data, channelNo, flag);
            return true;
        }

        public bool broadcastPacketTeam(TeamId team, GameServer.Logic.Packets.Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            return broadcastPacketTeam(team, packet.GetBytes(), channelNo, flag);
        }

        public bool broadcastPacketVision(GameObject o, GameServer.Logic.Packets.Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            return broadcastPacketVision(o, packet.GetBytes(), channelNo, flag);
        }

        public bool broadcastPacketVision(GameObject o, byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            var game = Program.ResolveDependency<Game>();
            foreach (var team in _teamsEnumerator)
            {
                if (team == TeamId.TEAM_NEUTRAL)
                    continue;

                if (game.Map.TeamHasVisionOn(team, o))
                {
                    broadcastPacketTeam(team, data, channelNo, flag);
                }
            }

            return true;
        }

        public bool handlePacket(Peer peer, byte[] data, Channel channelID)
        {
            var header = new GameServer.Logic.Packets.PacketHeader(data);
            var handler = GetHandler(header.cmd, channelID);

            switch (header.cmd)
            {
                case PacketCmd.PKT_C2S_StatsConfirm:
                case PacketCmd.PKT_C2S_MoveConfirm:
                case PacketCmd.PKT_C2S_ViewReq:
                    break;
                default:
                    break;
            }

            if (handler != null)
            {
                if (!handler.HandlePacket(peer, data))
                {
                    return false;
                }

                return true;
            }
            printPacket(data, "Error: ");
            return false;
        }

        public bool handlePacket(Peer peer, Packet packet, Channel channelID)
        {
            var data = new byte[(int)packet.Length];
            Marshal.Copy(packet.Data, data, 0, data.Length);

            if (data.Length >= 8)
                if (_playerManager.GetPeerInfo(peer) != null)
                    data = _blowfish.Decrypt(data);

            return handlePacket(peer, data, channelID);
        }
    }
}
