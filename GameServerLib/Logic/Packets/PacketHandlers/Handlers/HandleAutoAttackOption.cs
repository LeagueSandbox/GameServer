using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Chatbox;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleAutoAttackOption : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly ChatCommandManager _chatCommandManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_AutoAttackOption;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleAutoAttackOption(Game game, ChatCommandManager chatCommandManager)
        {
            _game = game;
            _chatCommandManager = chatCommandManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            //TODO: implement this
            var autoAttackOption = new AutoAttackOption(data);
            var state = "Deactivated";
            if (autoAttackOption.activated == 1)
                state = "Activated";

            _chatCommandManager.SendDebugMsgFormatted(ChatCommandManager.DebugMsgType.NORMAL, "Auto attack: " + state);
            return true;
        }
    }
}
