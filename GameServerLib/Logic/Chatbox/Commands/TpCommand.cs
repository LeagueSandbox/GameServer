using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Players;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class TpCommand : ChatCommandBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override string Command => "tp";
        public override string Syntax => $"{Command} x y";

        public TpCommand(ChatCommandManager chatCommandManager, Game game, PlayerManager playerManager) : base(chatCommandManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float x, y;
            if (split.Length < 3)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }
            if (float.TryParse(split[1], out x))
                if (float.TryParse(split[2], out y))
                    _game.PacketNotifier.NotifyTeleport(_playerManager.GetPeerInfo(peer).Champion, x, y);
        }
    }
}
