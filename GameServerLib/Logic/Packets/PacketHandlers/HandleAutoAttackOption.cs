using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Chatbox;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleAutoAttackOption : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly ChatCommandManager _chatCommandManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_AUTO_ATTACK_OPTION;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleAutoAttackOption(Game game)
        {
            _game = game;
            _chatCommandManager = game.ChatCommandManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            //TODO: implement this
            var autoAttackOption = _game.PacketReader.ReadAutoAttackOptionRequest(data);
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
