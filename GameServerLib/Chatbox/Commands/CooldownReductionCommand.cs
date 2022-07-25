using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class CooldownReductionCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "cdr";
        public override string Syntax => $"{Command} bonusCdr";

        public CooldownReductionCommand(ChatCommandManager chatCommandManager, Game game)
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
            else if (float.TryParse(split[1], out var cdr))
            {
                _playerManager.GetPeerInfo(userId).Champion.Stats.CooldownReduction.FlatBonus -= cdr / 100f;
            }
        }
    }
}
