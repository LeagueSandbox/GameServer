using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntWarsSharp.Core.Logic.PacketHandlers.Packets;
using IntWarsSharp.Core.Logic.PacketHandlers;
using ENet.Native;
using System.Runtime.InteropServices;
using ENet;
using IntWarsSharp.Logic.Enet;
using IntWarsSharp.Logic;

namespace IntWarsSharp.Core.Logic
{
    unsafe class PacketHandlerManager
    {
        private static PacketHandlerManager _instance;
        private Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>> _handlerTable;
        private Game game;

        internal void InitHandlers(Game g)
        {
            _handlerTable = new Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>>();

            registerHandler(new HandleKeyCheck(), PacketCmd.PKT_KeyCheck, Channel.CHL_HANDSHAKE);
            registerHandler(new HandleLoadPing(), PacketCmd.PKT_C2S_Ping_Load_Info, Channel.CHL_C2S);
            registerHandler(new HandleSpawn(), PacketCmd.PKT_C2S_CharLoaded, Channel.CHL_C2S);
            registerHandler(new HandleMap(), PacketCmd.PKT_C2S_ClientReady, Channel.CHL_LOADING_SCREEN);
            registerHandler(new HandleSynch(), PacketCmd.PKT_C2S_SynchVersion, Channel.CHL_C2S);
            registerHandler(new HandleCastSpell(), PacketCmd.PKT_C2S_CastSpell, Channel.CHL_C2S);
            //registerHandler(new HandleGameNumber(),      PacketCmd.PKT_C2S_GameNumberReq, Channel.CHL_C2S);
            registerHandler(new HandleQueryStatus(), PacketCmd.PKT_C2S_QueryStatusReq, Channel.CHL_C2S);
            registerHandler(new HandleStartGame(), PacketCmd.PKT_C2S_StartGame, Channel.CHL_C2S);
            registerHandler(new HandleNull(), PacketCmd.PKT_C2S_Exit, Channel.CHL_C2S);
            registerHandler(new HandleView(), PacketCmd.PKT_C2S_ViewReq, Channel.CHL_C2S);
            registerHandler(new HandleNull(), PacketCmd.PKT_C2S_Click, Channel.CHL_C2S);
            //registerHandler(new HandleNull(),            PacketCmd.PKT_C2S_OpenShop, Channel.CHL_C2S);
            registerHandler(new HandleAttentionPing(), PacketCmd.PKT_C2S_AttentionPing, Channel.CHL_C2S);
            registerHandler(new HandleChatBoxMessage(), PacketCmd.PKT_ChatBoxMessage, Channel.CHL_COMMUNICATION);
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

            game = g;
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

        public static PacketHandlerManager getInstace()
        {
            if (_instance == null)
                _instance = new PacketHandlerManager();

            return _instance;
        }

        internal IPacketHandler GetHandler(PacketCmd cmd, byte channelID)
        {
            if (_handlerTable.ContainsKey(cmd))
            {
                var handlers = _handlerTable[cmd];
                if (handlers.ContainsKey((Channel)channelID))
                    return handlers[(Channel)channelID];
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

        public void printPacket(byte[] buffer)
        {
            string hex = BitConverter.ToString(buffer);
            Console.WriteLine(hex.Replace("-", ""));
        }
        public bool sendPacket(ENetPeer* peer, byte[] source, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            ////PDEBUG_LOG_LINE(Logging," Sending packet:\n");
            //if(length < 300)
            //	printPacket(data, length);

            if (source.Length >= 8)
                source = game.getBlowfish().Encrypt(source, BlowFishMode.ECB); //Encrypt everything minus the last bytes that overflow the 8 byte boundary

            var ptr = allocMemory(source);
            var packet = ENetApi.enet_packet_create(ptr, new IntPtr(source.Length), flag);
            if (ENetApi.enet_peer_send(peer, (byte)channelNo, packet) < 0)
            {
                releaseMemory(ptr);
                return false;
            }

            releaseMemory(ptr);
            return true;
        }
        public bool broadcastPacket(byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            ////PDEBUG_LOG_LINE(Logging," Broadcast packet:\n");
            //printPacket(data, length);
            byte[] outData = new byte[data.Length - (data.Length % 8)];
            Array.Copy(data, outData, outData.Length); //not gonna work lmao

            if (outData.Length >= 8)// length - (length % 8)
                outData = game.getBlowfish().Encrypt(outData, BlowFishMode.ECB);

            var unmanagedPointer = allocMemory(outData);
            var packet = ENetApi.enet_packet_create(unmanagedPointer, new IntPtr(outData.Length), (PacketFlags)flag);

            ENetApi.enet_host_broadcast(game.getServer(), (byte)channelNo, packet);
            releaseMemory(unmanagedPointer);
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
            {
                if (o.isVisibleByTeam(i))
                {
                    broadcastPacketTeam((i == 0) ? TeamId.TEAM_BLUE : TeamId.TEAM_PURPLE, data, channelNo, flag);
                }
            }
            return true;
        }

        public bool handlePacket(ENetPeer* peer, ENetPacket* packet, byte channelID)
        {
            var data = new byte[(int)packet->dataLength - ((int)packet->dataLength % 8)];
            Marshal.Copy(packet->data, data, 0, data.Length); //not gonna work lmao

            if ((int)packet->dataLength >= 8)
                if (game.peerInfo(peer) != null)
                    data = game.getBlowfish().Decrypt(data, BlowFishMode.ECB); //Encrypt everything minus the last bytes that overflow the 8 byte boundary

            var header = new IntWarsSharp.Logic.Packets.PacketHeader(data);
            var handler = GetHandler(header.cmd, channelID);

            if (handler != null)
            {
                return handler.HandlePacket(peer, packet);
            }
            else
            {
                Logger.LogCoreWarning("Unhandled OpCode " + header.cmd);
                printPacket(data);
            }
            return false;
        }
    }
}
