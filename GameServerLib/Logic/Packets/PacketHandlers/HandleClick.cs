﻿using ENet;
using LeagueSandbox.GameServer.Logic.Attributes;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    [DisabledHandler]
    public class HandleClick : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly Logger _logger;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CLICK;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public HandleClick(Game game)
        {
            _game = game;
            _logger = game.GetLogger();
            _playerManager = game.GetPlayerManager();
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var click = new Click(data);
            var msg = $"Object {_playerManager.GetPeerInfo(peer).Champion.NetId} clicked on {click.TargetNetId}";
            _logger.LogCoreInfo(msg);

            return true;
        }
    }
}
