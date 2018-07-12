using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class GoldCommand : ChatCommandBase
    {
        public override string Command => "gold";
        public override string Syntax => $"{Command} goldAmount";

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (float.TryParse(split[1], out var gold))
            {
                PlayerManager.GetPeerInfo(peer).Champion.Stats.Gold += gold;
            }
        }
    }
}
