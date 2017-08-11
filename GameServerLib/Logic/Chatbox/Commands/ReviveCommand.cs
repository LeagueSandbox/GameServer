using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class ReviveCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "revive";
        public override string Syntax => "";

        public ReviveCommand(ChatCommandManager chatCommandManager, PlayerManager playerManager) : base(chatCommandManager)
        {
            _playerManager = playerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var champ = _playerManager.GetPeerInfo(peer).Champion;
            if (!champ.IsDead)
            {
                ChatCommandManager.SendDebugMsgFormatted(ChatCommandManager.DebugMsgType.INFO, "Your champion is already alive.");
                return;
            }
            ChatCommandManager.SendDebugMsgFormatted(ChatCommandManager.DebugMsgType.INFO, "Your champion has revived!");
            champ.Respawn();
        }
    }
}
