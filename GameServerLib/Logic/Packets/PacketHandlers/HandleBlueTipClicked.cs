using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
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
            var msg = $"Clicked blue tip with netid: {blueTipClicked.netid}";
            _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, msg);
            return true;
        }
    }
}
