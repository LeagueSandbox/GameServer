using GameServerCore;
using LeagueSandbox.GameServer.Logging;
using log4net;
using System.Numerics;
using System;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class CoordsCommand : ChatCommandBase
    {
        private readonly ILog _logger;
        private readonly IPlayerManager _playerManager;
        private readonly protected Game _game;

        public override string Command => "coords";
        public override string Syntax => $"{Command}";

        public CoordsCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _logger = LoggerProvider.GetLogger();
            _playerManager = game.PlayerManager;
            _game = game;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var champion = _playerManager.GetPeerInfo((ulong)userId).Champion;
            _logger.Debug($"At {champion.X}; {champion.Y}");
            var dirMsg = "Not moving anywhere";
            if (!champion.IsPathEnded())
            {
                Vector2 dir = champion.GetDirection();
                double ang = Math.Acos(dir.Y / dir.Length()) * (180 / Math.PI);
                dirMsg = $"dirX: {dir.X} dirY: {dir.Y} dirAngle: {ang}";
            }
            var msg = $"At Coords - X: {champion.X} Y: {champion.Y} Z: {champion.GetZ()} "+dirMsg;
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, msg);
        }
    }
}
