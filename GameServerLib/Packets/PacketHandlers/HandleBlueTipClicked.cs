using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Chatbox;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleBlueTipClicked : PacketHandlerBase<BlueTipClickedRequest>
    {
        private readonly Game _game;
        private readonly ChatCommandManager _chatCommandManager;
        private readonly IPlayerManager _playerManager;

        public HandleBlueTipClicked(Game game)
        {
            _game = game;
            _chatCommandManager = game.ChatCommandManager;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, BlueTipClickedRequest req)
        {
            // TODO: can we use player net id from request?
            var playerNetId = _playerManager.GetPeerInfo(userId).Champion.NetId;
            _game.PacketNotifier.NotifyS2C_HandleTipUpdatep(userId, "", "", "", 0, playerNetId, req.TipID);

            var msg = $"Clicked blue tip with netid: {req.TipID}";
            _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, msg);
            return true;
        }
    }
}
