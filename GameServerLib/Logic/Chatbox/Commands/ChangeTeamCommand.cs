using ENet;
using LeagueSandbox.GameServer.Logic.Players;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class ChangeTeamCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "changeteam";
        public override string Syntax => $"{Command} teamNumber";

        public ChangeTeamCommand(ChatCommandManager chatCommandManager, PlayerManager playerManager) : base(chatCommandManager)
        {
            _playerManager = playerManager;
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

            int t;
            if (!int.TryParse(split[1], out t))
            {
                return;
            }

            var team = CustomConvert.ToTeamId(t);
            _playerManager.GetPeerInfo(peer).Champion.SetTeam(team);
        }
    }
}
