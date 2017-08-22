﻿using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Attributes;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    [DisabledHandler]
    public class HandleClick : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly Logger _logger;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_Click;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleClick(Game game, Logger logger, PlayerManager playerManager)
        {
            _game = game;
            _logger = logger;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var click = new Click(data);
            var msg = $"Object {_playerManager.GetPeerInfo(peer).Champion.NetId} clicked on {click.targetNetId}";
            _logger.LogCoreInfo(msg);

            return true;
        }
    }
}
