using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class ApCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "ap";
        public override string Syntax => $"{Command} bonusAp";

        public ApCommand(ChatCommandManager chatCommandManager, PlayerManager playerManager)
            : base(chatCommandManager)
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
            }
            else if (float.TryParse(split[1], out var ap))
            {
                _playerManager.GetPeerInfo(peer).Champion.Stats.AbilityPower.FlatBonus += ap;
            }
        }
    }
}
