using ENet;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class SizeCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "size";
        public override string Syntax => $"{Command} size";

        public SizeCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

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
                _playerManager.GetPeerInfo(peer).Champion.Stats.Size.BaseValue += size;
            }
        }
    }
}
