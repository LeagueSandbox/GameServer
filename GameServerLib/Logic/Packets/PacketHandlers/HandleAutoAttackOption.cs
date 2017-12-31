using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleAutoAttackOption : PacketHandlerBase<AutoAttackOption>
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

        public override bool HandlePacketInternal(Peer peer, AutoAttackOption data)
        {
            //TODO: implement this
            var state = "Deactivated";
            if (data.Activated == 1)
                state = "Activated";

            _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, $"Auto attack: {state}");
            return true;
        }
    }
}
