using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Chatbox;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleQuestClicked : PacketHandlerBase
    {
        private readonly ChatCommandManager _chatCommandManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_QuestClicked;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleQuestClicked(ChatCommandManager chatCommandManager)
        {
            _chatCommandManager = chatCommandManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var questClicked = new QuestClicked(data);

            _chatCommandManager.SendDebugMsgFormatted(ChatCommandManager.DebugMsgType.NORMAL, $"Clicked quest with netid: {questClicked.netid}");
            return true;
        }
    }
}
