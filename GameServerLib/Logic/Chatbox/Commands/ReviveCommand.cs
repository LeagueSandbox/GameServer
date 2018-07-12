using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class ReviveCommand : ChatCommandBase
    {

        public override string Command => "revive";
        public override string Syntax => $"{Command}";

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var champ = PlayerManager.GetPeerInfo(peer).Champion;
            if (!champ.IsDead)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Your champion is already alive.");
                return;
            }

            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Your champion has revived!");
            champ.Respawn();
        }
    }
}
