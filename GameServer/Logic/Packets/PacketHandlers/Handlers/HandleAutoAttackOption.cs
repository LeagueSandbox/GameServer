using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleAutoAttackOption : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var autoAttackOption = new AutoAttackOption(data);
            var state = "Deactivated";
            if (autoAttackOption.activated == 1)
            {
                state = "Activated";
            }

            game.ChatboxManager.SendDebugMsgFormatted(GameServer.Logic.Chatbox.ChatboxManager.DebugMsgType.NORMAL, "Auto attack: " + state);
            return true;
        }
    }
}
