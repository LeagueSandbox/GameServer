using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class LevelCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "level";
        public override string Syntax => $"{Command} level";

        public LevelCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
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

                var experienceToLevelUp = Game.Map.MapGameScript.ExpToLevelUp[lvl - 1];
                _playerManager.GetPeerInfo(peer).Champion.Stats.Experience = experienceToLevelUp;
            }
        }
    }
}
