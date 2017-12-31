﻿using ENet;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleQuestClicked : PacketHandlerBase<QuestClicked>
    {
        private readonly ChatCommandManager _chatCommandManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_QuestClicked;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleQuestClicked(ChatCommandManager chatCommandManager)
        {
            _chatCommandManager = chatCommandManager;
        }

        public override bool HandlePacketInternal(Peer peer, QuestClicked data)
        {
            var msg = $"Clicked quest with netid: {data.QuestNetId}";
            _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, msg);
            return true;
        }
    }
}
