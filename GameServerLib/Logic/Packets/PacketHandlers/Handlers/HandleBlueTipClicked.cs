using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleBlueTipClicked : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly ChatCommandManager _chatCommandManager;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_BlueTipClicked;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleBlueTipClicked(Game game, ChatCommandManager chatCommandManager, PlayerManager playerManager)
        {
            _game = game;
            _chatCommandManager = chatCommandManager;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var blueTipClicked = new BlueTipClicked(data);
            var removeBlueTip = new BlueTip(
                "",
                "",
                "",
                0,
                _playerManager.GetPeerInfo(peer).Champion.NetId,
                blueTipClicked.netid);

            _game.PacketHandlerManager.sendPacket(peer, removeBlueTip, Channel.CHL_S2C);
            _chatCommandManager.SendDebugMsgFormatted(ChatCommandManager.DebugMsgType.NORMAL, $"Clicked blue tip with netid: {blueTipClicked.netid}");
            return true;
        }
    }
}
