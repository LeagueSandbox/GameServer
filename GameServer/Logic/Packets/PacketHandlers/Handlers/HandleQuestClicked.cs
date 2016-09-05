using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleQuestClicked : IPacketHandler
    {
        private ChatboxManager _chatboxManager = Program.ResolveDependency<ChatboxManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var questClicked = new QuestClicked(data);

            _chatboxManager.SendDebugMsgFormatted(
                GameServer.Logic.Chatbox.ChatboxManager.DebugMsgType.NORMAL,
                string.Format("Clicked quest with netid: {0}", questClicked.netid.ToString())
            );
            return true;
        }
    }
}
