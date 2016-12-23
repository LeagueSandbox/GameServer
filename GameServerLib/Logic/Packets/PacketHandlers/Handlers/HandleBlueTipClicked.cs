using ENet;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleBlueTipClicked : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private ChatCommandManager _chatCommandManager = Program.ResolveDependency<ChatCommandManager>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var blueTipClicked = new BlueTipClicked(data);
            var removeBlueTip = new BlueTip(
                "",
                "",
                "",
                0,
                _playerManager.GetPeerInfo(peer).Champion.NetId,
                blueTipClicked.netid
            );
            _game.PacketHandlerManager.sendPacket(peer, removeBlueTip, Channel.CHL_S2C);

            _chatCommandManager.SendDebugMsgFormatted(
                ChatCommandManager.DebugMsgType.NORMAL,
                string.Format("Clicked blue tip with netid: {0}", blueTipClicked.netid)
            );
            return true;
        }
    }
}
