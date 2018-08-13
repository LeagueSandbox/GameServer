using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Chatbox;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleQuestClicked : PacketHandlerBase
    {
        private readonly IPacketReader _packetReader;
        private readonly ChatCommandManager _chatCommandManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_QUEST_CLICKED;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleQuestClicked(Game game)
        {
            _packetReader = game.PacketReader;
            _chatCommandManager = game.ChatCommandManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _packetReader.ReadQuestClickedRequest(data);
            var msg = $"Clicked quest with netid: {request.QuestNetId}";
            _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, msg);
            return true;
        }
    }
}
