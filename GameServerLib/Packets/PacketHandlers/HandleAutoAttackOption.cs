using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;
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
            return true;
        }
    }
}
