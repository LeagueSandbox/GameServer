using ENet;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleQuestClicked : IPacketHandler
    {
        private ChatCommandManager _chatCommandManager = Program.ResolveDependency<ChatCommandManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var questClicked = new QuestClicked(data);

            _chatCommandManager.SendDebugMsgFormatted(
                GameServer.Logic.Chatbox.ChatCommandManager.DebugMsgType.NORMAL,
                string.Format("Clicked quest with netid: {0}", questClicked.netid.ToString())
            );
            return true;
        }
    }
}
