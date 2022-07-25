using LeagueSandbox.GameServer.Players;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class SpeedCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "speed";
        public override string Syntax => $"{Command} [flat speed] [percent speed]";

        public SpeedCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2 || split.Length > 3)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }

            try
            {
                StatsModifier stat = new StatsModifier();

                if (split.Length == 3)
                {
                    stat.MoveSpeed.PercentBonus = float.Parse(split[2]) / 100;
                }
                stat.MoveSpeed.FlatBonus = float.Parse(split[1]);

                _playerManager.GetPeerInfo(userId).Champion.AddStatModifier(stat);
            }
            catch
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
        }
    }
}
