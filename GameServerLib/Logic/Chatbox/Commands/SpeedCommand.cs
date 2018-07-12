using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class SpeedCommand : ChatCommandBase
    {

        public override string Command => "speed";
        public override string Syntax => $"{Command} speed";

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }

            if (float.TryParse(split[1], out var speed))
            {
                PlayerManager.GetPeerInfo(peer).Champion.Stats.MoveSpeed.FlatBonus += speed;
            }
            else
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.ERROR, "Incorrect parameter");
            }
        }
    }
}
