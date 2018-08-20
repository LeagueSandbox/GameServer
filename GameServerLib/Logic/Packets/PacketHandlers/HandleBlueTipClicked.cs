﻿using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleBlueTipClicked : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly ChatCommandManager _chatCommandManager;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_BLUE_TIP_CLICKED;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleBlueTipClicked(Game game)
        {
            _game = game;
            _chatCommandManager = game.ChatCommandManager;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _game.PacketReader.ReadBlueTipRequest(data);
            // TODO: can we use player net id from request?
            var playerNetId = _playerManager.GetPeerInfo(peer).Champion.NetId;
             _game.PacketNotifier.NotifyBlueTip(peer, "", "", "", 0, playerNetId, request.NetId);

            var msg = $"Clicked blue tip with netid: {request.NetId}";
            _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, msg);
            return true;
        }
    }
}
