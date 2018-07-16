using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class ReviveCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "revive";
        public override string Syntax => $"{Command}";

        public ReviveCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.GetPlayerManager();
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var champ = _playerManager.GetPeerInfo(peer).Champion;
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
