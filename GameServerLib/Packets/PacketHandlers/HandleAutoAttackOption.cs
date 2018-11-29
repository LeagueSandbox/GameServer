using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
using LeagueSandbox.GameServer.Chatbox;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleAutoAttackOption : PacketHandlerBase<AutoAttackOptionRequest>
    {
        private readonly Game _game;
        private readonly ChatCommandManager _chatCommandManager;

        public HandleAutoAttackOption(Game game)
        {
            _game = game;
            _chatCommandManager = game.ChatCommandManager;
        }

        public override bool HandlePacket(int userId, AutoAttackOptionRequest req)
        {
            var state = "Deactivated";
            if (req.Activated)
            {
                state = "Activated";
            }

            _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, $"Auto attack: {state}");
            return true;
        }
    }
}
