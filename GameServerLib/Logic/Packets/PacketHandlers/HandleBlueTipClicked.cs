using ENet;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleBlueTipClicked : PacketHandlerBase
    {
        private readonly IPacketReader _packetReader;
        private readonly IPacketNotifier _packetNotifier;
        private readonly Game _game;
        private readonly ChatCommandManager _chatCommandManager;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_BLUE_TIP_CLICKED;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleBlueTipClicked(Game game)
        {
            _packetReader = game.PacketReader;
            _packetNotifier = game.PacketNotifier;
            _game = game;
            _chatCommandManager = game.ChatCommandManager;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _packetReader.ReadBlueTipRequest(data);
            // TODO: can we use player net id from request?
            var playerNetId = _playerManager.GetPeerInfo(peer).Champion.NetId;
            _packetNotifier.NotifyBlueTip(peer, "", "", "", 0, playerNetId, request.NetId);
            
            var msg = $"Clicked blue tip with netid: {request.NetId}";
            _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, msg);
            return true;
        }
    }
}
