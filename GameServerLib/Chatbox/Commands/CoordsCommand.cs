using LeagueSandbox.GameServer.Players;
using LeagueSandbox.GameServer.Logging;
using log4net;
using System.Numerics;
using System;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class CoordsCommand : ChatCommandBase
    {
        private static ILog _logger = LoggerProvider.GetLogger();
        private readonly PlayerManager _playerManager;

        public override string Command => "coords";
        public override string Syntax => $"{Command}";

        public CoordsCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var champion = _playerManager.GetPeerInfo(userId).Champion;
            _logger.Debug($"At {champion.Position.X}; {champion.Position.Y}");
            var dirMsg = "Not moving anywhere";
            if (!champion.IsPathEnded())
            {
                Vector2 dir = new Vector2(champion.Direction.X, champion.Direction.Z);
                // Angle measured between [1, 0] and player direction vectors (to X axis)
                double ang = Math.Acos(dir.X / dir.Length()) * (180 / Math.PI); 
                dirMsg = $"dirX: {dir.X} dirY: {dir.Y} dirAngle (to X axis): {ang}";
            }
            var coords3D = champion.GetPosition3D();
            var msg = $"At Coords - X: {coords3D.X} Y: {coords3D.Z} Height: {coords3D.Y} "+dirMsg;
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, msg);
        }
    }
}
