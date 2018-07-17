using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class ChangeTeamCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "changeteam";
        public override string Syntax => $"{Command} teamNumber";

        public ChangeTeamCommand(ChatCommandManager chatCommandManager, Game game)
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
                return;
            }

            if (!int.TryParse(split[1], out var t))
            {
                return;
            }

            var team = CustomConvert.ToTeamId(t);
            _playerManager.GetPeerInfo(peer).Champion.SetTeam(team);
        }
    }
}
