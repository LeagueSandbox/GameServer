using ENet;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleAutoAttackOption : PacketHandlerBase
    {
        private readonly IPacketReader _packetReader;
        private readonly ChatCommandManager _chatCommandManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_AUTO_ATTACK_OPTION;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleAutoAttackOption(Game game)
        {
            _packetReader = game.PacketReader;
            _chatCommandManager = game.ChatCommandManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            //TODO: implement this
            var autoAttackOption = _packetReader.ReadAutoAttackOptionRequest(data);
            var state = "Deactivated";
            if (autoAttackOption.Activated)
            {
                state = "Activated";
            }

            _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, $"Auto attack: {state}");
            return true;
        }
    }
}
