using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class ChangeTeamCommand : ChatCommandBase
    {
        public override string Command => "changeteam";
        public override string Syntax => $"{Command} teamNumber";

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
            PlayerManager.GetPeerInfo(peer).Champion.SetTeam(team);
        }
    }
}
