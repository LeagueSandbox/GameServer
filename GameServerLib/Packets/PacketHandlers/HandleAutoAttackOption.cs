using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;
using LeagueSandbox.GameServer.Chatbox;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleAutoAttackOption : PacketHandlerBase<C2S_UpdateGameOptions>
    {
        private readonly Game _game;
        private readonly ChatCommandManager _chatCommandManager;

        public HandleAutoAttackOption(Game game)
        {
            _game = game;
            _chatCommandManager = game.ChatCommandManager;
        }

        public override bool HandlePacket(int userId, C2S_UpdateGameOptions req)
        {
            return true;
        }
    }
}
