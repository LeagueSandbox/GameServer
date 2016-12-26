using ENet;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleAutoAttackOption : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private ChatCommandManager _chatCommandManager = Program.ResolveDependency<ChatCommandManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var autoAttackOption = new AutoAttackOption(data);
            var state = "Deactivated";
            if (autoAttackOption.activated == 1)
            {
                state = "Activated";
            }

            _chatCommandManager.SendDebugMsgFormatted(
                GameServer.Logic.Chatbox.ChatCommandManager.DebugMsgType.NORMAL,
                "Auto attack: " + state
            );
            return true;
        }
    }
}
