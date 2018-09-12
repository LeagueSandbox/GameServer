using GameServerCore;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class ApCommand : ChatCommandBase
    {
        private readonly IPlayerManager _playerManager;

        public override string Command => "ap";
        public override string Syntax => $"{Command} bonusAp";

        public ApCommand(ChatCommandManager chatCommandManager, Game game)
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
            else if (float.TryParse(split[1], out var ap))
            {
                _playerManager.GetPeerInfo(userId).Champion.Stats.AbilityPower.FlatBonus += ap;
            }
        }
    }
}
