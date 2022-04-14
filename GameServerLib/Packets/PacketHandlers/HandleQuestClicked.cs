using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Chatbox;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleQuestClicked : PacketHandlerBase<QuestClickedRequest>
    {
        private readonly Game _game;
        private readonly ChatCommandManager _chatCommandManager;

        public HandleQuestClicked(Game game)
        {
            _game = game;
            _chatCommandManager = game.ChatCommandManager;
        }

        public override bool HandlePacket(int userId, QuestClickedRequest req)
        {
            var msg = $"Clicked quest with netid: {req.QuestID}";
            _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, msg);
            return true;
        }
    }
}
