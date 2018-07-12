using ENet;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleQuestClicked : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_QUEST_CLICKED;
        public override Channel PacketChannel => Channel.CHL_C2_S;
        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var questClicked = new QuestClicked(data);
            var msg = $"Clicked quest with netid: {questClicked.Netid}";
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, msg);
            return true;
        }
    }
}
