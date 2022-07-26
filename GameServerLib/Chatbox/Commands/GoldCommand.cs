using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class GoldCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "gold";
        public override string Syntax => $"{Command} goldAmount";

        public GoldCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (float.TryParse(split[1], out var gold))
            {
                var ch = _playerManager.GetPeerInfo(userId).Champion;
                ch.AddGold(ch, gold);
            }
        }
    }
}
