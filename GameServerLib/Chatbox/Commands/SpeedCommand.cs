using GameServerCore;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class SpeedCommand : ChatCommandBase
    {
        private readonly IPlayerManager _playerManager;

        public override string Command => "speed";
        public override string Syntax => $"{Command} parameter [p (percent), f (flat)] speed";

        public SpeedCommand(ChatCommandManager chatCommandManager, Game game)
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

            try
            {
                IStatsModifier stat = new StatsModifier();

                switch (split[1])
                {
                    case "p":
                        stat.MoveSpeed.PercentBonus = float.Parse(split[2]) / 100;
                        break;
                    case "f":
                        stat.MoveSpeed.FlatBonus = float.Parse(split[2]);
                        break;
                    default:
                        ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.ERROR, "Incorrect parameter");
                        break;
                }

                _playerManager.GetPeerInfo(userId).Champion.AddStatModifier(stat);
            }
            catch
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.ERROR, "Incorrect parameter");
            }
        }
    }
}
