using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class SizeCommand : ChatCommandBase
    {

        public override string Command => "size";
        public override string Syntax => $"{Command} size";

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float size;
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (float.TryParse(split[1], out size))
            {
                PlayerManager.GetPeerInfo(peer).Champion.Stats.Size.BaseValue += size;
            }
        }
    }
}
