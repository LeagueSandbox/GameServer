using GameServerCore;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class TpCommand : ChatCommandBase
    {
        private readonly IPlayerManager _playerManager;

        public override string Command => "tp";
        public override string Syntax => $"{Command} x y";

        public TpCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float x, y;
            if (split.Length < 3)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }

            if (float.TryParse(split[1], out x) && float.TryParse(split[2], out y))
            {
                _playerManager.GetPeerInfo(userId).Champion.TeleportTo(x, y);
            }
        }
    }
}
