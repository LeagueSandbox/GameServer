using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleQuestClicked : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var questClicked = new QuestClicked(data);

            game.ChatboxManager.SendDebugMsgFormatted(GameServer.Logic.Chatbox.ChatboxManager.DebugMsgType.NORMAL, "Clicked quest with netid: " + questClicked.netid.ToString());
            return true;
        }
    }
}
