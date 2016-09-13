using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleBlueTipClicked : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private ChatboxManager _chatboxManager = Program.ResolveDependency<ChatboxManager>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var blueTipClicked = new BlueTipClicked(data);
            var removeBlueTip = new BlueTip(
                "",
                "",
                _playerManager.GetPeerInfo(peer).Champion.NetId,
                blueTipClicked.netid,
                true
            );
            _game.PacketHandlerManager.sendPacket(peer, removeBlueTip, Channel.CHL_S2C);

            _chatboxManager.SendDebugMsgFormatted(
                GameServer.Logic.Chatbox.ChatboxManager.DebugMsgType.NORMAL,
                string.Format("Clicked blue tip with netid: {0}", blueTipClicked.netid.ToString())
            );
            return true;
        }
    }
}
