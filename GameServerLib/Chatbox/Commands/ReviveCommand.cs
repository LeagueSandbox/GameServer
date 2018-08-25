using ENet;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class ReviveCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "revive";
        public override string Syntax => $"{Command}";

        public ReviveCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var champ = (Champion)_playerManager.GetPeerInfo(peer).Champion;
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
