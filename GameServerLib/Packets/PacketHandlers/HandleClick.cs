﻿using GameServerCore;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
using LeagueSandbox.GameServer.Attributes;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    [DisabledHandler]
    public class HandleClick : PacketHandlerBase<ClickRequest>
    {
        private readonly Game _game;
        private readonly ILog _logger;
        private readonly IPlayerManager _playerManager;

        public HandleClick(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, ClickRequest req)
        {
            var msg = $"Object {_playerManager.GetPeerInfo((ulong)userId).Champion.NetId} clicked on {req.TargetNetId}";
            _logger.Debug(msg);

            return true;
        }
    }
}
