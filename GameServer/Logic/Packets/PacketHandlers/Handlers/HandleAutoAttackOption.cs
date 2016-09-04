using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleAutoAttackOption : IPacketHandler
    {
        private ChatboxManager _chatboxManager = Program.ResolveDependency<ChatboxManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var autoAttackOption = new AutoAttackOption(data);
            var state = "Deactivated";
            if (autoAttackOption.activated == 1)
            {
                state = "Activated";
            }

            _chatboxManager.SendDebugMsgFormatted(
                GameServer.Logic.Chatbox.ChatboxManager.DebugMsgType.NORMAL,
                "Auto attack: " + state
            );
            return true;
        }
    }
}
