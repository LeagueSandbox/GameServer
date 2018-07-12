using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class HealthCommand : ChatCommandBase
    {public override string Command => "health";
        public override string Syntax => $"{Command} maxHealth";

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (float.TryParse(split[1], out var hp))
            {
                PlayerManager.GetPeerInfo(peer).Champion.Stats.HealthPoints.FlatBonus += hp;
                PlayerManager.GetPeerInfo(peer).Champion.Stats.CurrentHealth += hp;
            }
        }
    }
}
