using GameServerCore;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class LevelCommand : ChatCommandBase
    {
        private readonly IPlayerManager _playerManager;

        public override string Command => "level";
        public override string Syntax => $"{Command} level";
        private readonly MapData _mapData;

        public LevelCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
            _mapData = game.Config.MapData;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (byte.TryParse(split[1], out var lvl))
            {
                if (lvl < 1 || lvl > 18)
                {
                    return;
                }

                var experienceToLevelUp = _mapData.ExpCurve[lvl - 1];
                _playerManager.GetPeerInfo(userId).Champion.Stats.Experience = experienceToLevelUp;
            }
        }
    }
}
