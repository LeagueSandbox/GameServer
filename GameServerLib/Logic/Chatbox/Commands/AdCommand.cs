using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class AdCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "ad";
        public override string Syntax => $"{Command} bonusAd";

        public AdCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (float.TryParse(split[1], out var ad))
            {
                _playerManager.GetPeerInfo(peer).Champion.Stats.AttackDamage.FlatBonus += ad;
            }
        }
    }
}
