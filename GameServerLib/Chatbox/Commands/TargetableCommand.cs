using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class TargettableCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;
        public override string Command => "targetable";
        public override string Syntax => $"{Command} false (untargetable) / true (targetable)";

        public TargettableCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');

            if (split.Length != 2 || !bool.TryParse(split[1], out var userInput))
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else
            {
                _playerManager.GetPeerInfo(userId).Champion.SetStatus(GameServerCore.Enums.StatusFlags.Targetable, userInput);
            }
        }
    }
}