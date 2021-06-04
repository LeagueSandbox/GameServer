﻿using GameServerCore;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSyncSimTime : PacketHandlerBase<SyncSimTimeRequest>
    {
        private readonly Game _game;
        private readonly ILog _logger;
        private readonly IPlayerManager _playerManager;

        public HandleSyncSimTime(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, SyncSimTimeRequest req)
        {
            var diff = req.AckTime - req.ReceiveTime;
            if (req.ReceiveTime > req.AckTime)
            {
                var peerInfo = _playerManager.GetPeerInfo(userId);
                var msg = $"Player {peerInfo.PlayerId} sent an invalid heartbeat - Timestamp error (diff: {diff})";
                _logger.Warn(msg);
            }

            return true;
        }
    }
}
