using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntWarsSharp.Core.Logic.PacketHandlers.Packets;
using IntWarsSharp.Core.Logic.PacketHandlers;
using static ENet.Native;
using System.Runtime.InteropServices;
using ENet;
using IntWarsSharp.Logic.Enet;
using IntWarsSharp.Logic;

namespace IntWarsSharp.Core.Logic
{
    unsafe class PacketHandlerManager
    {
        private static PacketHandlerManager _instance;
        private Dictionary<PacketCmdC2S, Dictionary<Channel, IPacketHandler>> _handlerTable;
        private Game game;

        internal void InitHandlers(Game g)
        {
            _handlerTable = new Dictionary<PacketCmdC2S, Dictionary<Channel, IPacketHandler>>();

            registerHandler(new HandleKeyCheck(), PacketCmdC2S.PKT_C2S_KeyCheck, Channel.CHL_HANDSHAKE);
            registerHandler(new HandleLoadPing(), PacketCmdC2S.PKT_C2S_Ping_Load_Info, Channel.CHL_C2S);
            registerHandler(new HandleSpawn(), PacketCmdC2S.PKT_C2S_CharLoaded, Channel.CHL_C2S);
            registerHandler(new HandleMap(), PacketCmdC2S.PKT_C2S_ClientReady, Channel.CHL_LOADING_SCREEN);
            registerHandler(new HandleSynch(), PacketCmdC2S.PKT_C2S_SynchVersion, Channel.CHL_C2S);
            registerHandler(new HandleCastSpell(), PacketCmdC2S.PKT_C2S_CastSpell, Channel.CHL_C2S);
            //registerHandler(new HandleGameNumber(),      PacketCmd.PKT_C2S_GameNumberReq, Channel.CHL_C2S);
            registerHandler(new HandleQueryStatus(), PacketCmdC2S.PKT_C2S_QueryStatusReq, Channel.CHL_C2S);
            registerHandler(new HandleStartGame(), PacketCmdC2S.PKT_C2S_StartGame, Channel.CHL_C2S);
            registerHandler(new HandleNull(), PacketCmdC2S.PKT_C2S_Exit, Channel.CHL_C2S);
            registerHandler(new HandleView(), PacketCmdC2S.PKT_C2S_ViewReq, Channel.CHL_C2S);
            registerHandler(new HandleNull(), PacketCmdC2S.PKT_C2S_Click, Channel.CHL_C2S);
            //registerHandler(new HandleNull(),            PacketCmd.PKT_C2S_OpenShop, Channel.CHL_C2S);
            registerHandler(new HandleAttentionPing(), PacketCmdC2S.PKT_C2S_AttentionPing, Channel.CHL_C2S);
            registerHandler(new HandleChatBoxMessage(), PacketCmdC2S.PKT_C2S_ChatBoxMessage, Channel.CHL_COMMUNICATION);
            registerHandler(new HandleMove(), PacketCmdC2S.PKT_C2S_MoveReq, Channel.CHL_C2S);
            registerHandler(new HandleNull(), PacketCmdC2S.PKT_C2S_MoveConfirm, Channel.CHL_C2S);
            registerHandler(new HandleSkillUp(), PacketCmdC2S.PKT_C2S_SkillUp, Channel.CHL_C2S);
            registerHandler(new HandleEmotion(), PacketCmdC2S.PKT_C2S_Emotion, Channel.CHL_C2S);
            registerHandler(new HandleBuyItem(), PacketCmdC2S.PKT_C2S_BuyItemReq, Channel.CHL_C2S);
            registerHandler(new HandleSellItem(), PacketCmdC2S.PKT_C2S_SellItem, Channel.CHL_C2S);
            registerHandler(new HandleSwapItems(), PacketCmdC2S.PKT_C2S_SwapItems, Channel.CHL_C2S);
            registerHandler(new HandleNull(), PacketCmdC2S.PKT_C2S_LockCamera, Channel.CHL_C2S);
            registerHandler(new HandleNull(), PacketCmdC2S.PKT_C2S_StatsConfirm, Channel.CHL_C2S);
            registerHandler(new HandleClick(), PacketCmdC2S.PKT_C2S_Click, Channel.CHL_C2S);
            registerHandler(new HandleHeartBeat(), PacketCmdC2S.PKT_C2S_HeartBeat, Channel.CHL_GAMEPLAY);
            //registerHandler(new ?, PacketCmdC2S.PKT_C2S_PauseReq, Channel.?);

            game = g;
        }

        public void registerHandler(IPacketHandler handler, PacketCmdC2S pktcmd, Channel channel)
        {

            if (!_handlerTable.ContainsKey(pktcmd))
                _handlerTable.Add(pktcmd, new Dictionary<Channel, IPacketHandler>());

            var dict = _handlerTable[pktcmd];
            if (!dict.ContainsKey(channel))
                dict.Add(channel, handler);
            else
                dict[channel] = handler;
        }

        public static PacketHandlerManager getInstace()
        {
            if (_instance == null)
                _instance = new PacketHandlerManager();

            return _instance;
        }

        internal IPacketHandler GetHandler(PacketCmdC2S cmd, Channel channelID)
        {
            if (_handlerTable.ContainsKey(cmd))
            {
                var handlers = _handlerTable[cmd];
                if (handlers.ContainsKey(channelID))
                    return handlers[channelID];
            }
            return null;
        }
        public bool sendPacket(ENetPeer* peer, IntWarsSharp.Logic.Packets.Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
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
                    System.Diagnostics.Debug.Write(b.ToString("x") + " ");
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("--------");
            }
        }
        public bool sendPacket(ENetPeer* peer, byte[] source, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            ////PDEBUG_LOG_LINE(Logging," Sending packet:\n");
            //if(length < 300)
            //printPacket(source, "Sent: ");

            fixed (byte* data = source)
            {
                if (source.Length >= 8)
                    BlowFishCS.BlowFishCS.Encrypt1(game.getBlowfish(), data, new IntPtr(source.Length - (source.Length % 8)));

                var packet = enet_packet_create(new IntPtr(data), new IntPtr(source.Length), flag);
                if (enet_peer_send(peer, (byte)channelNo, packet) < 0)
                    return false;
            }
            return true;
        }
        public bool broadcastPacket(byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            ////PDEBUG_LOG_LINE(Logging," Broadcast packet:\n");
            //printPacket(data, "Broadcast: ");

            fixed (byte* b = data)
            {
                if (data.Length >= 8)
                    BlowFishCS.BlowFishCS.Encrypt1(game.getBlowfish(), b, new IntPtr(data.Length - (data.Length % 8)));

                var packet = enet_packet_create(new IntPtr(b), new IntPtr(data.Length), (PacketFlags)flag);
                enet_host_broadcast(game.getServer(), (byte)channelNo, packet);
            }
            return true;
        }

        public bool broadcastPacket(IntWarsSharp.Logic.Packets.Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            return broadcastPacket(packet.GetBytes(), channelNo, flag);
        }


        public bool broadcastPacketTeam(TeamId team, byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            foreach (var ci in game.getPlayers())
                if (ci.Item2.getPeer() != null && ci.Item2.getTeam() == team)
                    sendPacket(ci.Item2.getPeer(), data, channelNo, flag);
            return true;
        }

        public bool broadcastPacketTeam(TeamId team, IntWarsSharp.Logic.Packets.Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            return broadcastPacketTeam(team, packet.GetBytes(), channelNo, flag);
        }

        public bool broadcastPacketVision(GameObject o, IntWarsSharp.Logic.Packets.Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            return broadcastPacketVision(o, packet.GetBytes(), channelNo, flag);
        }

        public bool broadcastPacketVision(GameObject o, byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            for (int i = 0; i < 2; ++i)
                if (o.isVisibleByTeam(Convert.toTeamId(i)))
                    broadcastPacketTeam(Convert.toTeamId(i), data, channelNo, flag);
            return true;
        }

        public bool handlePacket(ENetPeer* peer, byte[] data, Channel channelID)
        {
            var header = new IntWarsSharp.Logic.Packets.PacketHeader(data);
            var handler = GetHandler(header.cmd, channelID);
            // printPacket(data, "Received: ");
            if (header.cmd != PacketCmdC2S.PKT_C2S_StatsConfirm && header.cmd != PacketCmdC2S.PKT_C2S_MoveConfirm)
                Console.WriteLine("Requested " + header.cmd.ToString());

            if (handler != null)
            {
                if (!handler.HandlePacket(peer, data, game))
                {
                    Console.WriteLine("Handle failed for " + header.cmd.ToString());
                    return false;
                }
                return true;
            }
            else
            {
                Logger.LogCoreWarning("Unhandled OpCode " + header.cmd);
                printPacket(data, "Error: ");
            }
            return false;
        }

        public bool handlePacket(ENetPeer* peer, ENetPacket* packet, Channel channelID)
        {
            if ((int)packet->dataLength >= 8)
                if (game.getPeerInfo(peer) != null)
                    BlowFishCS.BlowFishCS.Decrypt1(game.getBlowfish(), (byte*)packet->data, new IntPtr((int)packet->dataLength - ((int)packet->dataLength % 8)));

            var data = new byte[(int)packet->dataLength];
            Marshal.Copy(packet->data, data, 0, data.Length);

            return handlePacket(peer, data, channelID);
        }
    }
}
