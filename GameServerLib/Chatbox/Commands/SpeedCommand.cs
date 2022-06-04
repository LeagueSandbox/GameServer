using GameServerCore;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class SpeedCommand : ChatCommandBase
    {
        private readonly IPlayerManager _playerManager;

        public override string Command => "speed";
        public override string Syntax => $"{Command} speed";

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

            if (float.TryParse(split[1], out var speed))
            {
                IStatsModifier stat = new StatsModifier();
                stat.MoveSpeed.FlatBonus = speed;
                _playerManager.GetPeerInfo(userId).Champion.AddStatModifier(stat);
            }
            else
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.ERROR, "Incorrect parameter");
            }
        }
    }
}
