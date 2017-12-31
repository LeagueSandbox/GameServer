﻿using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleUseObject : PacketHandlerBase<UseObject>
    {
        private readonly Game _game;
        private readonly Logger _logger;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_UseObject;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleUseObject(Game game, Logger logger, PlayerManager playerManager)
        {
            _game = game;
            _logger = logger;
            _playerManager = playerManager;
        }

        public override bool HandlePacketInternal(Peer peer, UseObject data)
        {
            var champion = _playerManager.GetPeerInfo(peer).Champion;
            var msg = $"Object {champion.NetId} is trying to use (right clicked) {data.TargetNetId}";
            _logger.LogCoreInfo(msg);

            return true;
        }
    }
}
