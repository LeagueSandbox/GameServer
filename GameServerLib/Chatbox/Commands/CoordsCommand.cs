using GameServerCore;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class CoordsCommand : ChatCommandBase
    {
        private readonly ILog _logger;
        private readonly IPlayerManager _playerManager;

        public override string Command => "coords";
        public override string Syntax => $"{Command}";

        public CoordsCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _logger = LoggerProvider.GetLogger();
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var champion = _playerManager.GetPeerInfo(peer).Champion;
            _logger.Debug($"At {champion.Position.X}; {champion.Position.Y}");
            var msg = $"At Coords - X: {champion.Position.X} Y: {champion.Position.Y} Z: {champion.GetZ()}";
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, msg);
        }
    }
}
