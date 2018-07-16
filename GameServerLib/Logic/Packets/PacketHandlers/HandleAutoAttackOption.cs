using ENet;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleAutoAttackOption : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly ChatCommandManager _chatCommandManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_AUTO_ATTACK_OPTION;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public HandleAutoAttackOption(Game game)
        {
            _game = game;
            _chatCommandManager = game.GetChatCommandManager();
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            //TODO: implement this
            var autoAttackOption = new AutoAttackOption(data);
            var state = "Deactivated";
            if (autoAttackOption.Activated == 1)
            {
                state = "Activated";
            }

            _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, $"Auto attack: {state}");
            return true;
        }
    }
}
