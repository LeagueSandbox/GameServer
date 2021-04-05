using GameServerCore;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class HealthCommand : ChatCommandBase
    {
        private readonly IPlayerManager _playerManager;

        public override string Command => "health";
        public override string Syntax => $"{Command} maxHealth";

        public HealthCommand(ChatCommandManager chatCommandManager, Game game)
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
            else if (float.TryParse(split[1], out var hp))
            {
                _playerManager.GetPeerInfo(userId).Champion.Stats.HealthPoints.FlatBonus += hp;
                _playerManager.GetPeerInfo(userId).Champion.Stats.CurrentHealth += hp;
            }
        }
    }
}
