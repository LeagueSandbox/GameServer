using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntWarsSharp.Core.Logic.PacketHandlers.Packets;
using IntWarsSharp.Core.Logic.PacketHandlers;

namespace IntWarsSharp.Core.Logic
{
    class PacketHandlerManager
    {
        private static PacketHandlerManager _instance;
        private Dictionary<PacketCmd, Dictionary<Channel, PacketHandler>> _handlerTable;

        internal void InitHandlers()
        {
            _handlerTable = new Dictionary<PacketCmd, Dictionary<Channel, PacketHandler>>();
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
        }

        public void registerHandler(PacketHandler handler, PacketCmd pktcmd, Channel channel)
        {

            if (!_handlerTable.ContainsKey(pktcmd))
                _handlerTable.Add(pktcmd, new Dictionary<Channel, PacketHandler>());

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
    }
}
