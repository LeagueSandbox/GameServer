using GameServerCore;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class ModelCommand : ChatCommandBase
    {
        private readonly IPlayerManager _playerManager;

        public override string Command => "model";
        public override string Syntax => $"{Command} modelName";

        public ModelCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.Split(' ');
            if (split.Length >= 2)
            {
                _playerManager.GetPeerInfo(userId).Champion.ChangeModel(split[1]);
            }
            else
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
        }
    }
}
